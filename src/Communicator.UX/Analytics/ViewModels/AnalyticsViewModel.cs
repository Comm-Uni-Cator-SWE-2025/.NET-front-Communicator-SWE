using System.Collections.ObjectModel;
using System.Windows;
using Communicator.Controller.RPC;
using Communicator.UX.Analytics.Models;
using Communicator.UX.Analytics.Services;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Communicator.UX.Core.Models;
using Timer = System.Timers.Timer;

namespace Communicator.UX.Analytics.ViewModels;

public class AnalyticsViewModel : ObservableObject, IDisposable
{
    // 
    // GRAPH MODELS
    // 
    public GraphViewModel Graph1_AI { get; } = new();
    public CanvasGraphViewModel Graph2_Canvas { get; } = new();
    public GraphViewModel Graph3_Msg { get; } = new();
    public GraphViewModel Graph4_Screen { get; } = new("FPS");

    // 
    // SERVICES
    // 
    private readonly ApiService? _aiService;
    private readonly AIMessageService? _msgService;
    private readonly CanvasDataService _canvasService = new();
    private readonly ScreenShareService? _screenService;
    private readonly IThemeService? _themeService;

    // 
    // TIMERS
    // 
    private readonly Timer _aiTimer;
    private readonly Timer _msgTimer;
    private readonly Timer _screenTimer;

    // 
    // STATE
    // 
    private int _canvasIndex = 1;

    public ObservableCollection<string> MessageList { get; } = new();

    // 
    // CONSTRUCTOR
    // 
    public AnalyticsViewModel(IThemeService? themeService = null, IRpcEventService? rpcEventService = null, IRPC? rpc = null)
    {
        _themeService = themeService;

        // Initialize services with RPC if available
        if (rpc != null)
        {
            _aiService = new ApiService(rpc);
            _msgService = new AIMessageService(rpc);
            _screenService = new ScreenShareService(rpc);
        }

        if (_themeService != null)
        {
            _themeService.ThemeChanged += OnThemeChanged;
        }

        if (rpcEventService != null)
        {
            System.Diagnostics.Debug.WriteLine("RPC event service registeration happens");
        }

        // AI Graph Timer (1 minute) - calls core/AiSentiment
        _aiTimer = new Timer(60 * 1000);
        _aiTimer.Elapsed += async (_, _) => await UpdateAI();
        _aiTimer.Start();
        _ = UpdateAI();

        // Message Timer (1 minute) - calls core/AiAction
        _msgTimer = new Timer(60 * 1000);
        _msgTimer.Elapsed += async (_, _) => await UpdateMessages();
        _msgTimer.Start();
        _ = UpdateMessages();

        // ScreenShare Timer (3 seconds) - calls core/ScreenTelemetry
        _screenTimer = new Timer(3000);
        _screenTimer.Elapsed += async (_, _) => await UpdateScreenShare();
        _screenTimer.Start();
        _ = UpdateScreenShare();

        // Subscribe to Canvas Data Updates
        CanvasDataService.CanvasDataChanged += OnCanvasDataReceived;

        // Apply initial theme
        ApplyTheme();
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(ApplyTheme);
    }

    public void ApplyTheme()
    {
        Graph1_AI.ApplyTheme();
        Graph2_Canvas.ApplyTheme();
        Graph3_Msg.ApplyTheme();
        Graph4_Screen.ApplyTheme();
    }

    public void Dispose()
    {
        if (_themeService != null)
        {
            _themeService.ThemeChanged -= OnThemeChanged;
        }

        CanvasDataService.CanvasDataChanged -= OnCanvasDataReceived;

        _aiTimer.Stop();
        _msgTimer.Stop();
        _screenTimer.Stop();

        _aiTimer.Dispose();
        _msgTimer.Dispose();
        _screenTimer.Dispose();
    }

    // 
    // GRAPH 1 — AI SENTIMENT (Dynamic) - Fetches from core/AiSentiment via RPC
    //
    private async Task UpdateAI()
    {
        if (_aiService == null)
        {
            System.Diagnostics.Debug.WriteLine("AI Service not initialized - RPC not available");
            return;
        }

        try
        {
            // Fetch new sentiment data from RPC
            List<AIData> newData = await _aiService.FetchAIDataAsync();

            if (newData.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No new sentiment data received");
                return;
            }

            // Add new points to the graph with time labels
            foreach (AIData d in newData)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    Graph1_AI.AddPointWithLabel(d.TimeLabel, d.Value);
                });
            }

            System.Diagnostics.Debug.WriteLine($"Added {newData.Count} new sentiment points to graph");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating AI sentiment: {ex.Message}");
        }
    }

    //
    // GRAPH 2 — Canvas Snapshot
    //
    private void OnCanvasDataReceived(CanvasData data)
    {
        Application.Current.Dispatcher.Invoke(() => {
            Graph2_Canvas.AddSnapshot(data, $"T{_canvasIndex}");
            _canvasIndex++;
        });
    }

    // 
    // GRAPH 3 — AI Action Items - Fetches from core/AiAction via RPC
    //
    private async Task UpdateMessages()
    {
        if (_msgService == null)
        {
            System.Diagnostics.Debug.WriteLine("Message Service not initialized - RPC not available");
            return;
        }

        try
        {
            // Fetch new action items from RPC
            List<AIMessageData> newMessages = await _msgService.FetchNextAsync();

            if (newMessages.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No new action items received");
                return;
            }

            // Add new messages to the list (skip duplicates)
            Application.Current.Dispatcher.Invoke(() => {
                foreach (AIMessageData m in newMessages)
                {
                    // Check if action already exists (case-insensitive)
                    if (!MessageList.Any(existing => string.Equals(existing, m.Message, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageList.Add(m.Message);
                    }
                }
            });

            System.Diagnostics.Debug.WriteLine($"Added {newMessages.Count} new action items");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating action items: {ex.Message}");
        }
    }

    // 
    // GRAPH 4 — Screen/Video Telemetry (FPS) - Fetches from core/ScreenTelemetry via RPC
    // 
    private async Task UpdateScreenShare()
    {
        if (_screenService == null)
        {
            System.Diagnostics.Debug.WriteLine("ScreenShare Service not initialized - RPC not available");
            return;
        }

        try
        {
            // Fetch new telemetry data from RPC
            List<ScreenVideoTelemetryModel> newTelemetry = await _screenService.FetchTelemetryAsync();

            if (newTelemetry.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No new screen telemetry received");
                return;
            }

            // Add FPS data points to the graph with time labels
            Application.Current.Dispatcher.Invoke(() => {
                foreach (ScreenVideoTelemetryModel entry in newTelemetry)
                {
                    DateTime startTime = entry.StartDateTime;

                    // Add each FPS reading (every 3 seconds)
                    for (int i = 0; i < entry.FpsEvery3Seconds.Count; i++)
                    {
                        DateTime pointTime = startTime.AddSeconds(i * 3);
                        string timeLabel = pointTime.ToString("HH:mm:ss");
                        double fps = entry.FpsEvery3Seconds[i];

                        Graph4_Screen.AddPointWithLabel(timeLabel, fps);
                    }
                }
            });

            System.Diagnostics.Debug.WriteLine($"Added telemetry from {newTelemetry.Count} entries");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating screen telemetry: {ex.Message}");
        }
    }
}
