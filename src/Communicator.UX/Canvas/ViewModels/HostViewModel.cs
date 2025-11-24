// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * -----------------------------------------------------------------------------
 *  File: HostViewModel.cs
 *  Owner: Pranitha Muluguru
 *  Roll Number : 112201019
 *  Module : Canvas
 *
 * -----------------------------------------------------------------------------
 */
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using Communicator.Canvas;
using Communicator.Cloud.CloudFunction.DataStructures;
using Communicator.Cloud.CloudFunction.FunctionLibrary;
using Communicator.Controller.Meeting;
using Communicator.Controller.Serialization;
using Communicator.Core.RPC;
using Communicator.Core.UX.Services;
using Microsoft.Win32;

namespace Communicator.UX.Canvas.ViewModels;

/// <summary>
/// Represents the Host (Server) side logic for the collaborative canvas.
/// Manages action validation, broadcasting to clients, and cloud autosaving.
/// </summary>
public class HostViewModel : CanvasViewModel
{
    /// <summary>
    /// Timer for triggering the cloud auto-save functionality.
    /// </summary>
    private bool _suppressCommit = false;

    /// <summary>
    /// Timer for triggering the cloud auto-save functionality.
    /// </summary>
    private System.Timers.Timer _autoSaveTimer;

    /// <summary>
    /// Indicates that this instance is the Host.
    /// </summary>
    public override bool IsHost => true;


    /// <summary>
    /// Initializes the HostViewModel, registers network listeners, and starts the auto-save timer.
    /// </summary>
    public HostViewModel(UserProfile user, IRPC rpc, IRpcEventService rpcEventService) : base(rpc, rpcEventService)
    {
        CurrentUserId = user.DisplayName ?? "Host";

        // --- Initialize and Start Auto-Save Timer ---
        _autoSaveTimer = new System.Timers.Timer(1 * 30 * 1000);
        _autoSaveTimer.Elapsed += async (sender, e) => await CloudSave();
        _autoSaveTimer.AutoReset = true;
        _autoSaveTimer.Start();

        LogToDesktop("HostViewModel initialized. Auto-save timer started.");
    }

    public override void ReceiveData(byte[] data)
    {
        string json = DataSerializer.Deserialize<string>(data);
        System.Windows.Application.Current.Dispatcher.Invoke(() => ProcessIncomingMessage(json));
    }



    /// <summary>
    /// Writes a debug message to a text file on the Desktop.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private void LogToDesktop(string message)
    {
        try
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "canvas_debug_log.txt");
            File.AppendAllText(path, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        catch
        {
            /* Ignore logging errors */
        }
    }

    /// <summary>
    /// Helper to safely show Pop-ups from background threads via the Dispatcher.
    /// </summary>
    /// <param name="message">The body text of the message box.</param>
    /// <param name="title">The title of the message box.</param>
    /// <param name="icon">The icon to display.</param>
    private void ShowPopup(string message, string title, MessageBoxImage icon)
    {
        Application.Current.Dispatcher.Invoke(() => {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        });
    }

    // --- Public Methods (Cloud & Network) ---

    public CloudFunctionLibrary _cloud = new CloudFunctionLibrary();
    /// <summary>
    /// Serializes the current state and posts it to the Cloud.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task CloudSave()
    {
        try
        {
            string shapesJson = CanvasSerializer.SerializeShapesDictionary(_shapes);

            Entity postEntity = new Entity(
                Module: "Canvas",
                Table: "Snapshots",
                Id: Guid.NewGuid().ToString(),
                Type: "post",
                LastN: -1,
                TimeRange: new TimeRange(0, 0),
                Data: JsonDocument.Parse(shapesJson).RootElement
            );

            CloudResponse response = await _cloud.CloudPostAsync(postEntity);

            // Write to log file on Desktop
            LogToDesktop($"Auto-save executed. Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            LogToDesktop($"Auto-save FAILED: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads the most recent snapshot from the cloud and saves it to the Desktop.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task DownloadLastCloudSnapshot()
    {
        try
        {

            Entity getEntity = new Entity(
                Module: "Canvas",
                Table: "Snapshots",
                Id: "",
                Type: "",
                LastN: 1,
                TimeRange: null,
                Data: JsonDocument.Parse("{}").RootElement
            );

            CloudResponse response = await _cloud.CloudGetAsync(getEntity);

            // 1. Check if we got data
            if (response.Data.ValueKind == JsonValueKind.Undefined || response.Data.ValueKind == JsonValueKind.Null)
            {
                ShowPopup($"Cloud returned empty data. Status: {response.StatusCode}", "Warning", MessageBoxImage.Warning);
                return;
            }

            string finalJsonToSave = "";

            // 2. Unwrap the Array (LastN returns a list)
            if (response.Data.ValueKind == JsonValueKind.Array)
            {
                if (response.Data.GetArrayLength() > 0)
                {
                    JsonElement firstEntity = response.Data[0];

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
                finalJsonToSave = response.Data.ToString();
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

    /// <summary>
    /// Commits modifications, suppressed if needed to avoid recursion.
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
    /// Processes a local action, validates it, and broadcasts it to clients.
    /// </summary>
    /// <param name="action">The action performed locally.</param>
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
            }
        }

        if (ValidateAction(action))
        {
            ApplyActionLocally(action);
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.NORMAL, action);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            byte[] data = DataSerializer.Serialize(json);

            // Broadcast to all clients via Java Backend
            Rpc.Call("canvas:broadcast", data);
        }
        else
        {
            Console.WriteLine($"[Host] Local Action Rejected: {action.ActionType} on {action.NewShape?.ShapeId}");
            RaiseRequestRedraw();
        }
    }

    /// <summary>
    /// Undo an action locally and broadcast the inverse action to clients.
    /// </summary>
    public override void Undo()
    {
        CommitModification();
        SelectedShape = null;
        CanvasAction? actionToUndo = StateManager.PeekUndo();
        base.Undo();

        if (actionToUndo != null)
        {
            CanvasAction reverseAction = GetInverseAction(actionToUndo, CurrentUserId);
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.UNDO, reverseAction);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            byte[] data = DataSerializer.Serialize(json);

            // Broadcast to all clients via Java Backend
            Rpc.Call("canvas:broadcast", data);
        }
    }

    /// <summary>
    /// Redo an action locally and broadcast it to clients.
    /// </summary>
    public override void Redo()
    {
        SelectedShape = null;
        CanvasAction? actionToRedo = StateManager.PeekRedo();
        base.Redo();

        if (actionToRedo != null)
        {
            NetworkMessage msg = new NetworkMessage(NetworkMessageType.REDO, actionToRedo);
            string json = CanvasSerializer.SerializeNetworkMessage(msg);
            byte[] data = DataSerializer.Serialize(json);

            // Broadcast to all clients via Java Backend
            Rpc.Call("canvas:broadcast", data);
        }
    }

    /// <summary>
    /// Restores the canvas from a local file and broadcasts the state to clients.
    /// </summary>
    public void RestoreShapes()
    {
        OpenFileDialog openDialog = new OpenFileDialog {
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
                NetworkMessage msg = new NetworkMessage(NetworkMessageType.RESTORE, null, json);
                string networkJson = CanvasSerializer.SerializeNetworkMessage(msg);

                Console.WriteLine("[Host] Broadcasting RESTORE command...");
                byte[] data = DataSerializer.Serialize(networkJson);

                // Broadcast to all clients via Java Backend
                Rpc.Call("canvas:broadcast", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Host] Failed to restore: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Processes incoming messages (rare for Host, as it is the authority).
    /// </summary>
    /// <param name="json">The serialized message.</param>
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
                    byte[] data = DataSerializer.Serialize(json);

                    // Broadcast to all clients via Java Backend
                    Rpc.Call("canvas:broadcast", data);
                }
                else
                {
                    Console.WriteLine($"[Host] Validation Failed for Incoming Action: {action.ActionType}");
                }
            }
        }
        // Host ignores incoming RESTORE messages (it is the source)
    }

    /// <summary>
    /// Validates an action against the current state to prevent conflicts.
    /// </summary>
    /// <param name="action">The action to validate.</param>
    /// <returns>True if the action is valid, otherwise False.</returns>
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
