// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * -----------------------------------------------------------------------------
 *  File: ClientViewModel.cs
 *  Owner: Sami Mohiddin
 *  Roll Number : 132201032
 *  Module : Canvas
 *
 * -----------------------------------------------------------------------------
 */
using System.Text;
using Communicator.Canvas;
using Communicator.Core.RPC;
using Communicator.Networking;

namespace Communicator.UX.Canvas.ViewModels;

/// <summary>
/// Represents the Client-side logic for the collaborative canvas.
/// Handles sending actions to the host and processing incoming network messages.
/// </summary>
public class ClientViewModel : CanvasViewModel, IMessageListener
{
    private readonly INetworking _networking;

    /// <summary>
    /// The IP address of the host/server machine.
    /// </summary>
    private string _hostIp = "";

    private int _hostPort = 0;

    /// <summary>
    /// Flag to prevent infinite loops when committing changes triggered by network updates.
    /// </summary>
    private bool _suppressCommit = false;

    private const int CanvasModuleId = 2;


    /// <summary>
    /// Initializes a new instance of the ClientViewModel.
    /// Registers the network listener and assigns a unique user ID.
    /// </summary>
    public ClientViewModel(INetworking networking, IRPC rpc) : base(rpc)
    {
        _networking = networking;
        CurrentUserId = "Client_" + Guid.NewGuid().ToString().Substring(0, 4);

        _networking.Subscribe(CanvasModuleId, this);
    }

    public async void Initialize()
    {
        await InitializeHostIp();
    }

    private async Task InitializeHostIp()
    {
        try
        {
            byte[] response = await _rpc.Call("canvas:getHostIp", Array.Empty<byte>());
            System.Diagnostics.Debug.WriteLine("Received host IP response from RPC: " + Encoding.UTF8.GetString(response));
            string hostString = Encoding.UTF8.GetString(response);
            string[] parts = hostString.Split(':');
            if (parts.Length == 2)
            {
                _hostIp = parts[0];
                System.Diagnostics.Debug.WriteLine("Parsed host IP: " + _hostIp);
                if (int.TryParse(parts[1], out int port))
                {
                    _hostPort = port;
                    System.Diagnostics.Debug.WriteLine("Parsed host port: " + _hostPort);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Failed to get host IP: {ex.Message}");
        }
    }

    public void ReceiveData(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        // Marshal to UI thread if necessary, but ViewModels usually handle property changes on UI thread automatically if bound correctly.
        // However, ReceiveData comes from background thread.
        System.Windows.Application.Current.Dispatcher.Invoke(() => ProcessIncomingMessage(json));
    }


    /// <summary>
    /// Commits the current modification to the state.
    /// Suppressed if the change is coming from the network to avoid loops.
    /// </summary>
    public override void CommitModification()
    {
        if (_suppressCommit)
        {
            return;
        }

        base.CommitModification();
    }

    /// <summary>
    /// Processes an action locally and sends it to the Host.
    /// </summary>
    /// <param name="action">The action performed by the user.</param>
    protected override void ProcessAction(CanvasAction action)
    {
        if (action.ActionType == CanvasActionType.Modify && action.PrevShape != null)
        {
            if (_shapes.ContainsKey(action.PrevShape.ShapeId))
            {
                _shapes[action.PrevShape.ShapeId] = action.PrevShape;
                if (SelectedShape != null && SelectedShape.ShapeId == action.PrevShape.ShapeId)
                {
                    _suppressCommit = true;
                    try
                    {
                        base.SelectedShape = action.PrevShape;
                    }
                    finally
                    {
                        _suppressCommit = false;
                    }
                }
                RaiseRequestRedraw();
            }
        }

        NetworkMessage msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);

        if (!string.IsNullOrEmpty(_hostIp))
        {
            byte[] data = Encoding.UTF8.GetBytes(json);
            ClientNode hostNode = new ClientNode(_hostIp, _hostPort);
            _networking.SendData(data, new[] { hostNode }, CanvasModuleId, 1);
        }

        ShowGhostShape(action);
    }

    /// <summary>
    /// Sends an Undo request to the Host.
    /// </summary>
    public override void Undo()
    {
        CommitModification();
        SelectedShape = null;
        CanvasAction? actionToUndo = _stateManager.PeekUndo();

        if (actionToUndo != null)
        {
            CanvasAction reverseAction = GetInverseAction(actionToUndo, CurrentUserId);
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.UNDO, reverseAction);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);

            if (!string.IsNullOrEmpty(_hostIp))
            {
                byte[] data = Encoding.UTF8.GetBytes(json);
                ClientNode hostNode = new ClientNode(_hostIp, _hostPort);
                _networking.SendData(data, new[] { hostNode }, CanvasModuleId, 1);
            }
        }
    }

    /// <summary>
    /// Sends a Redo request to the Host.
    /// </summary>
    public override void Redo()
    {
        SelectedShape = null;
        CanvasAction? actionToRedo = _stateManager.PeekRedo();

        if (actionToRedo != null)
        {
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.REDO, actionToRedo);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            if (!string.IsNullOrEmpty(_hostIp))
            {
                byte[] data = Encoding.UTF8.GetBytes(json);
                ClientNode hostNode = new ClientNode(_hostIp, _hostPort);
                _networking.SendData(data, new[] { hostNode }, CanvasModuleId, 1);
            }
        }
    }


    /// <summary>
    /// Displays a temporary "ghost" shape to give immediate feedback before server confirmation.
    /// </summary>
    /// <param name="action">The action containing the shape to display.</param>
    private async void ShowGhostShape(CanvasAction action)
    {
        if (action.NewShape != null)
        {
            IShape ghost = action.NewShape;
            GhostShapes.Add(ghost);
            RaiseRequestRedraw();
            await Task.Delay(2000);
            if (GhostShapes.Contains(ghost))
            {
                GhostShapes.Remove(ghost);
                RaiseRequestRedraw();
            }
        }
    }

    /// <summary>
    /// Handles incoming JSON messages from the network.
    /// </summary>
    /// <param name="json">The serialized network message.</param>
    public void ProcessIncomingMessage(string json)
    {
        NetworkMessage? msg = CanvasSerializer.DeserializeNetworkMessage(json);
        if (msg == null)
        {
            return;
        }

        if (msg.MessageType == NetworkMessageType.RESTORE)
        {
            Console.WriteLine("[Client] Received RESTORE command.");
            if (!string.IsNullOrEmpty(msg.Payload))
            {
                ApplyRestore(msg.Payload);
            }
            return;
        }

        CanvasAction action = msg.Action;
        if (action == null)
        {
            return;
        }

        bool isMyAction = false;

        if (msg.MessageType == NetworkMessageType.NORMAL)
        {
            isMyAction = action.NewShape?.LastModifiedBy == CurrentUserId;
        }
        else
        {
            if (msg.MessageType == NetworkMessageType.UNDO)
            {
                CanvasAction? localUndo = _stateManager.PeekUndo();
                if (localUndo != null && localUndo.ActionId == action.ActionId)
                {
                    isMyAction = true;
                }
            }
            else if (msg.MessageType == NetworkMessageType.REDO)
            {
                CanvasAction? localRedo = _stateManager.PeekRedo();
                if (localRedo != null && localRedo.ActionId == action.ActionId)
                {
                    isMyAction = true;
                }
            }
        }

        switch (msg.MessageType)
        {
            case NetworkMessageType.NORMAL:
                HandleNormalMessage(action, isMyAction);
                break;
            case NetworkMessageType.UNDO:
                HandleUndoMessage(action, isMyAction);
                break;
            case NetworkMessageType.REDO:
                HandleRedoMessage(action, isMyAction);
                break;
        }
    }

    /// <summary>
    /// Handles a standard Create/Modify/Delete message from the Host.
    /// </summary>
    private void HandleNormalMessage(CanvasAction action, bool isMyAction)
    {
        if (isMyAction)
        {
            ApplyActionLocally(action);
            IShape? ghost = GhostShapes.FirstOrDefault(g => g.ShapeId == action.NewShape?.ShapeId);
            if (ghost != null)
            {
                GhostShapes.Remove(ghost);
                RaiseRequestRedraw();
            }
        }
        else
        {
            if (action.NewShape != null)
            {
                UpdateShapeFromNetwork(action.NewShape);
                RaiseRequestRedraw();
            }
        }
    }

    /// <summary>
    /// Handles an Undo message from the Host.
    /// </summary>
    private void HandleUndoMessage(CanvasAction action, bool isMyAction)
    {
        if (isMyAction)
        {
            _stateManager.Undo();
        }
        if (action.NewShape != null)
        {
            UpdateShapeFromNetwork(action.NewShape);
        }
        RaiseRequestRedraw();
    }

    /// <summary>
    /// Handles a Redo message from the Host.
    /// </summary>
    private void HandleRedoMessage(CanvasAction action, bool isMyAction)
    {
        if (isMyAction)
        {
            _stateManager.Redo();
        }
        if (action.NewShape != null)
        {
            UpdateShapeFromNetwork(action.NewShape);
        }
        RaiseRequestRedraw();
    }
}
