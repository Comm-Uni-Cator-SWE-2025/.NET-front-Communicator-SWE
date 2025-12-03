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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Communicator.Canvas;
using Communicator.Controller.Meeting;
using Communicator.Controller.Serialization;
using Communicator.Controller.RPC;
using Communicator.UX.Core.Services;
using Communicator.Networking;

namespace Communicator.UX.Canvas.ViewModels;

/// <summary>
/// Represents the Client-side logic for the collaborative canvas.
/// Handles sending actions to the host and processing incoming network messages.
/// </summary>
public class ClientViewModel : CanvasViewModel
{
    /// Need to decide we need to put or remove according to java
    /// <summary>
    /// The IP address of the host/server machine.
    /// </summary>
    //private string _hostIp = "";

    //private int _hostPort = 0;

    /// <summary>
    /// Flag to prevent infinite loops when committing changes triggered by network updates.
    /// </summary>
    private bool _suppressCommit = false;

    private const int CanvasModuleId = 2;


    /// <summary>
    /// Initializes a new instance of the ClientViewModel.
    /// Registers the network listener and assigns a unique user ID.
    /// </summary>
    public ClientViewModel(UserProfile user, IRPC rpc, IRpcEventService rpcEventService) : base(rpc, rpcEventService)
    {
        CurrentUserId = user.DisplayName ?? "Client_" + Guid.NewGuid().ToString().Substring(0, 4);
    }

    public async void Initialize()
    {
        // Request history from the backend/host
        try
        {
            System.Diagnostics.Debug.WriteLine("[CanvasClientModel] Requesting canvas shapes...");
            byte[] whoAmIResponse = await Rpc.Call("canvas:whoami", Array.Empty<byte>());
            ClientNode myClientNode = DataSerializer.Deserialize<ClientNode>(whoAmIResponse);

            if (myClientNode == null)
            {
                System.Diagnostics.Debug.WriteLine("[CanvasClientModel] Failed to get identity from canvas:whoami");
                return;
            }

            byte[] payloadBytes = DataSerializer.Serialize(myClientNode);
            string payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

            NetworkMessage netMsg = new NetworkMessage(NetworkMessageType.REQUEST_SHAPES, null, payloadJson);

            string json = CanvasSerializer.SerializeNetworkMessage(netMsg);
            byte[] data = DataSerializer.Serialize(json);

            await Rpc.Call("canvas:sendToHost", data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CanvasClientModel] Failed to get history: {ex.Message}");
        }
    }

    public override void ReceiveData(byte[] data)
    {
        string json = DataSerializer.Deserialize<string>(data);
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
        byte[] data = DataSerializer.Serialize(json);

        // Send to Host for verification
        Rpc.Call("canvas:sendToHost", data);

        ShowGhostShape(action);
    }

    /// <summary>
    /// Sends an Undo request to the Host.
    /// </summary>
    public override void Undo()
    {
        CommitModification();
        SelectedShape = null;
        CanvasAction? actionToUndo = StateManager.PeekUndo();

        if (actionToUndo != null)
        {
            CanvasAction reverseAction = GetInverseAction(actionToUndo, CurrentUserId);
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.UNDO, reverseAction);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);

            byte[] data = DataSerializer.Serialize(json);

            // Send to Host for verification
            Rpc.Call("canvas:sendToHost", data);
        }
    }

    /// <summary>
    /// Sends a Redo request to the Host.
    /// </summary>
    public override void Redo()
    {
        SelectedShape = null;
        CanvasAction? actionToRedo = StateManager.PeekRedo();

        if (actionToRedo != null)
        {
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.REDO, actionToRedo);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            byte[] data = DataSerializer.Serialize(json);

            // Send to Host for verification
            Rpc.Call("canvas:sendToHost", data);
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
        Console.WriteLine("[CanvasClientModel] Processing incoming message...");
        Console.WriteLine(json);
        NetworkMessage? msg = CanvasSerializer.DeserializeNetworkMessage(json);
        Console.WriteLine("[CanvasClientModel] Deserialized message:");
        Console.WriteLine(msg);
        if (msg == null)
        {
            return;
        }

        if (msg.MessageType == NetworkMessageType.RESTORE)
        {
            System.Diagnostics.Debug.WriteLine("[CanvasClientModel] Received RESTORE command.");
            if (!string.IsNullOrEmpty(msg.Payload))
            {
                ApplyRestore(msg.Payload);
            }
            return;
        }

        CanvasAction? action = msg.Action;
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
                CanvasAction? localUndo = StateManager.PeekUndo();
                if (localUndo != null && localUndo.ActionId == action.ActionId)
                {
                    isMyAction = true;
                }
            }
            else if (msg.MessageType == NetworkMessageType.REDO)
            {
                CanvasAction? localRedo = StateManager.PeekRedo();
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
            StateManager.Undo();
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
            StateManager.Redo();
        }
        if (action.NewShape != null)
        {
            UpdateShapeFromNetwork(action.NewShape);
        }
        RaiseRequestRedraw();
    }
}
