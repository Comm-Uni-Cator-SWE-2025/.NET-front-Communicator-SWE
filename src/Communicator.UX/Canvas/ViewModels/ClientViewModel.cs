using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communicator.Canvas;
using Communicator.Core.RPC;
using Communicator.Networking;
using Microsoft.Win32;

namespace Communicator.UX.Canvas.ViewModels;

public class ClientViewModel : CanvasViewModel, IMessageListener
{
    private readonly INetworking _networking;
    private string _hostIp = "";
    private int _hostPort = 0;
    private bool _suppressCommit = false;
    private const int CanvasModuleId = 2;

    public ClientViewModel(INetworking networking, IRPC rpc) : base(rpc)
    {
        _networking = networking;
        CurrentUserId = "Client_" + Guid.NewGuid().ToString().Substring(0, 4);
        
        _networking.Subscribe(CanvasModuleId, this);
        InitializeHostIp();
    }

    private async void InitializeHostIp()
    {
        try
        {
            byte[] response = await _rpc.Call("canvas:getHostIp", Array.Empty<byte>());
            string hostString = Encoding.UTF8.GetString(response);
            string[] parts = hostString.Split(':');
            if (parts.Length == 2)
            {
                _hostIp = parts[0];
                if (int.TryParse(parts[1], out int port))
                {
                    _hostPort = port;
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

    public override void CommitModification()
    {
        if (_suppressCommit)
        {
            return;
        }

        base.CommitModification();
    }

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
                    try { base.SelectedShape = action.PrevShape; }
                    finally { _suppressCommit = false; }
                }
                RaiseRequestRedraw();
            }
        }

        var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
        string json = CanvasSerializer.SerializeNetworkMessage(msg);
        
        if (!string.IsNullOrEmpty(_hostIp))
        {
            byte[] data = Encoding.UTF8.GetBytes(json);
            ClientNode hostNode = new ClientNode(_hostIp, _hostPort);
            _networking.SendData(data, new[] { hostNode }, CanvasModuleId, 1);
        }
        
        ShowGhostShape(action);
    }

    public override void Undo()
    {
        CommitModification();
        SelectedShape = null;
        CanvasAction? actionToUndo = _stateManager.PeekUndo();

        if (actionToUndo != null)
        {
            CanvasAction reverseAction = GetInverseAction(actionToUndo, CurrentUserId);
            var msg = new NetworkMessage(NetworkMessageType.UNDO, reverseAction);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            
            if (!string.IsNullOrEmpty(_hostIp))
            {
                byte[] data = Encoding.UTF8.GetBytes(json);
                ClientNode hostNode = new ClientNode(_hostIp, _hostPort);
                _networking.SendData(data, new[] { hostNode }, CanvasModuleId, 1);
            }
        }
    }

    public override void Redo()
    {
        SelectedShape = null;
        CanvasAction? actionToRedo = _stateManager.PeekRedo();

        if (actionToRedo != null)
        {
            var msg = new NetworkMessage(NetworkMessageType.REDO, actionToRedo);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            
            if (!string.IsNullOrEmpty(_hostIp))
            {
                byte[] data = Encoding.UTF8.GetBytes(json);
                ClientNode hostNode = new ClientNode(_hostIp, _hostPort);
                _networking.SendData(data, new[] { hostNode }, CanvasModuleId, 1);
            }
        }
    }

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

    public void ProcessIncomingMessage(string json)
    {
        NetworkMessage? msg = CanvasSerializer.DeserializeNetworkMessage(json);
        if (msg == null)
        {
            return;
        }

        // --- HANDLE RESTORE ---
        if (msg.MessageType == NetworkMessageType.RESTORE)
        {
            Console.WriteLine("[Client] Received RESTORE command.");
            if (!string.IsNullOrEmpty(msg.Payload))
            {
                ApplyRestore(msg.Payload);
            }
            return;
        }
        // ---------------------

        CanvasAction action = msg.Action;
        if (action == null)
        {
            return; // Safety
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
            case NetworkMessageType.NORMAL: HandleNormalMessage(action, isMyAction); break;
            case NetworkMessageType.UNDO: HandleUndoMessage(action, isMyAction); break;
            case NetworkMessageType.REDO: HandleRedoMessage(action, isMyAction); break;
        }
    }

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
                // KEY FIX: Use helper to ensure SelectedShape isn't stale
                UpdateShapeFromNetwork(action.NewShape);
                RaiseRequestRedraw();
            }
        }
    }

    private void HandleUndoMessage(CanvasAction action, bool isMyAction)
    {
        if (isMyAction)
        {
            _stateManager.Undo();
        }
        if (action.NewShape != null)
        {
            // KEY FIX
            UpdateShapeFromNetwork(action.NewShape);
        }
        RaiseRequestRedraw();
    }

    private void HandleRedoMessage(CanvasAction action, bool isMyAction)
    {
        if (isMyAction)
        {
            _stateManager.Redo();
        }
        if (action.NewShape != null)
        {
            // KEY FIX
            UpdateShapeFromNetwork(action.NewShape);
        }
        RaiseRequestRedraw();
    }
}
