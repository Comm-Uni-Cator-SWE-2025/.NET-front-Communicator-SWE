using System;
using System.Collections.Generic;
using System.Text.Json;
using Communicator.Canvas;
using Microsoft.Win32;
namespace Communicator.UX.Canvas.ViewModels;
using System.IO; // Still keep this
using System.Threading.Tasks;
using System.Timers; // Required for the Timer
using System.Windows;
using Communicator.Cloud.CloudFunction;
using Communicator.Cloud.CloudFunction.DataStructures;
using Communicator.Cloud.CloudFunction.FunctionLibrary;

public class HostViewModel : CanvasViewModel
{

    private readonly string _myIp = "127.0.0.1";
    private readonly List<string> _clientIps = new() { "192.168.1.50" };
    private bool _suppressCommit = false;
    private Timer _autoSaveTimer;
    // --- OVERRIDE TO TRUE ---
    public override bool IsHost => true;
    // ------------------------

    public HostViewModel()
    {
        CurrentUserId = "Host_Admin";
        NetworkMock.Register(_myIp, ProcessIncomingMessage);

        // --- Initialize and Start Auto-Save Timer ---
        // Set interval to 5 minutes
        _autoSaveTimer = new Timer(30 * 1000);
        _autoSaveTimer.Elapsed += async (sender, e) => await CloudSave();
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();

        // Log startup to text file on Desktop
        LogToDesktop("HostViewModel initialized. Auto-save timer started.");
    }

    // --- Helper to verify background tasks without Console ---
    // This creates a text file on your Desktop called "canvas_debug_log.txt"
    private void LogToDesktop(string message)
    {
        try
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "canvas_debug_log.txt");
            File.AppendAllText(path, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        catch { /* Ignore logging errors */ }
    }

    // --- Helper to safely show Pop-ups from background threads ---
    private void ShowPopup(string message, string title, MessageBoxImage icon)
    {
        Application.Current.Dispatcher.Invoke(() => {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        });
    }

    public async Task CloudSave()
    {
        try
        {
            string shapesJson = CanvasSerializer.SerializeShapesDictionary(_shapes);
            CloudFunctionLibrary _cloud = new CloudFunctionLibrary();

            Entity _postEntity = new Entity(
                Module: "Canvas",
                Table: "Snapshots",
                Id: Guid.NewGuid().ToString(),
                Type: "post",
                LastN: -1,
                TimeRange: new TimeRange(0, 0),
                Data: JsonDocument.Parse(shapesJson).RootElement
            );

            CloudResponse _response = await _cloud.CloudPostAsync(_postEntity);

            // Write to log file on Desktop
            LogToDesktop($"Auto-save executed. Status: {_response.StatusCode}");
        }
        catch (Exception ex)
        {
            LogToDesktop($"Auto-save FAILED: {ex.Message}");
        }
    }

    // --- UPDATED: Retrieve Last Snapshot ---
    // --- UPDATED: Retrieve Last Snapshot with Parsing Logic ---
    public async Task DownloadLastCloudSnapshot()
    {
        try
        {
            CloudFunctionLibrary _cloud = new CloudFunctionLibrary();

            Entity _getEntity = new Entity(
                Module: "Canvas",
                Table: "Snapshots",
                Id: "",
                Type: "",
                LastN: 1,
                TimeRange: null,
                Data: JsonDocument.Parse("{}").RootElement
            );

            CloudResponse _response = await _cloud.CloudGetAsync(_getEntity);

            // 1. Check if we got data
            if (_response.Data.ValueKind == JsonValueKind.Undefined || _response.Data.ValueKind == JsonValueKind.Null)
            {
                ShowPopup($"Cloud returned empty data. Status: {_response.StatusCode}", "Warning", MessageBoxImage.Warning);
                return;
            }

            string finalJsonToSave = "";

            // 2. Unwrap the Array (LastN returns a list)
            if (_response.Data.ValueKind == JsonValueKind.Array)
            {
                if (_response.Data.GetArrayLength() > 0)
                {
                    JsonElement firstEntity = _response.Data[0];

                    // 3. Extract the inner "data" property which holds our shapes
                    // We try both "data" and "Data" to be safe
                    if (firstEntity.TryGetProperty("data", out JsonElement innerData) ||
                        firstEntity.TryGetProperty("Data", out innerData))
                    {
                        finalJsonToSave = innerData.ToString();
                    }
                    else
                    {
                        // Fallback: If no "data" property, save the whole entity
                        finalJsonToSave = firstEntity.ToString();
                    }
                }
                else
                {
                    ShowPopup("Cloud returned an empty list (Array length 0).", "Info", MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                // Not an array, just use it directly
                finalJsonToSave = _response.Data.ToString();
            }

            // 4. Save to Desktop
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = $"Canvas_Restore_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string fullPath = Path.Combine(desktopPath, fileName);

            File.WriteAllText(fullPath, finalJsonToSave);

            ShowPopup(
                $"Snapshot successfully downloaded!\nLocation: {fullPath}",
                "Success",
                MessageBoxImage.Information);

        }
        catch (Exception ex)
        {
            ShowPopup(
                $"Error during download/save:\n{ex.Message}\n{ex.StackTrace}",
                "Critical Error",
                MessageBoxImage.Error);
        }
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
            }
        }

        if (ValidateAction(action))
        {
            ApplyActionLocally(action);
            var msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            NetworkMock.Broadcast(_clientIps, json);
        }
        else
        {
            Console.WriteLine($"[Host] Local Action Rejected: {action.ActionType} on {action.NewShape?.ShapeId}");
            RaiseRequestRedraw();
        }
    }

    public override void Undo()
    {
        CommitModification();
        SelectedShape = null;
        CanvasAction? actionToUndo = _stateManager.PeekUndo();
        base.Undo();

        if (actionToUndo != null)
        {
            CanvasAction reverseAction = GetInverseAction(actionToUndo, CurrentUserId);
            var msg = new NetworkMessage(NetworkMessageType.UNDO, reverseAction);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            NetworkMock.Broadcast(_clientIps, json);
        }
    }

    public override void Redo()
    {
        SelectedShape = null;
        CanvasAction? actionToRedo = _stateManager.PeekRedo();
        base.Redo();

        if (actionToRedo != null)
        {
            var msg = new NetworkMessage(NetworkMessageType.REDO, actionToRedo);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            NetworkMock.Broadcast(_clientIps, json);
        }
    }

    //public void ProcessIncomingMessage(string json)
    //{
    //    NetworkMessage? msg = CanvasSerializer.DeserializeNetworkMessage(json);
    //    if (msg == null) return;

    //    CanvasAction action = msg.Action;

    //    if (ValidateAction(action))
    //    {
    //        if (action.NewShape != null)
    //        {
    //            // KEY FIX: Use helper to sync dictionary and selection
    //            UpdateShapeFromNetwork(action.NewShape);
    //        }

    //        RaiseRequestRedraw();
    //        NetworkMock.Broadcast(_clientIps, json);
    //    }
    //    else
    //    {
    //        Console.WriteLine($"[Host] Validation Failed for Incoming Action: {action.ActionType}");
    //    }
    //}
    // --- NEW: RESTORE FEATURE ---
    // --- NEW: RESTORE FEATURE ---
    public void RestoreShapes()
    {
        OpenFileDialog openDialog = new OpenFileDialog
        {
            Filter = "Canvas JSON (*.json)|*.json"
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                string json = File.ReadAllText(openDialog.FileName);

                // 1. Apply Locally
                ApplyRestore(json);

                // 2. Broadcast RESTORE Message
                var msg = new NetworkMessage(NetworkMessageType.RESTORE, null, json);
                string networkJson = CanvasSerializer.SerializeNetworkMessage(msg);

                Console.WriteLine("[Host] Broadcasting RESTORE command...");
                NetworkMock.Broadcast(_clientIps, networkJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Host] Failed to restore: {ex.Message}");
            }
        }
    }
    // ----------------------------

    public void ProcessIncomingMessage(string json)
    {
        NetworkMessage? msg = CanvasSerializer.DeserializeNetworkMessage(json);
        if (msg == null)
        {
            return;
        }

        // Host usually only receives its own broadcasts in this setup, or client actions
        // Logic kept for Client Actions
        if (msg.MessageType == NetworkMessageType.NORMAL || msg.MessageType == NetworkMessageType.UNDO || msg.MessageType == NetworkMessageType.REDO)
        {
            if (msg.Action != null)
            {
                CanvasAction action = msg.Action;
                if (ValidateAction(action))
                {
                    if (action.NewShape != null)
                    {
                        UpdateShapeFromNetwork(action.NewShape);
                    }

                    RaiseRequestRedraw();
                    NetworkMock.Broadcast(_clientIps, json);
                }
                else
                {
                    Console.WriteLine($"[Host] Validation Failed for Incoming Action: {action.ActionType}");
                }
            }
        }
        // Host ignores incoming RESTORE messages (it is the source)
    }
    private bool ValidateAction(CanvasAction action)
    {
        string shapeId = action.NewShape?.ShapeId ?? action.PrevShape?.ShapeId ?? "";
        if (string.IsNullOrEmpty(shapeId))
        {
            return false;
        }

        switch (action.ActionType)
        {
            case CanvasActionType.Create:
                return true;

            case CanvasActionType.Delete:
            case CanvasActionType.Modify:
            case CanvasActionType.Resurrect:
                if (!_shapes.ContainsKey(shapeId))
                {
                    return false;
                }

                IShape currentHostShape = _shapes[shapeId];
                IShape? incomingPrevShape = action.PrevShape;

                if (incomingPrevShape == null)
                {
                    return false;
                }

                if (currentHostShape.ShapeId != incomingPrevShape.ShapeId)
                {
                    return false;
                }

                if (currentHostShape.LastModifiedBy != incomingPrevShape.LastModifiedBy)
                {
                    Console.WriteLine($"[Host] Version Mismatch! HostVer: {currentHostShape.LastModifiedBy}, IncomingVer: {incomingPrevShape.LastModifiedBy}");
                    return false;
                }

                return true;

            default:
                return false;
        }
    }
}
