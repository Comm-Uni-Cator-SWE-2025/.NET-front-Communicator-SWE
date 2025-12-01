using System.Collections.ObjectModel;
using System.Windows;
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
    public GraphViewModel Graph4_Screen { get; } = new();

    // 
    // SERVICES
    // 
    private readonly ApiService _aiService = new();
    private readonly AIMessageService _msgService = new();
    private readonly CanvasDataService _canvasService = new();
    private readonly ScreenShareService _screenService = new();   //s
    private readonly IThemeService? _themeService;

    // 
    // TIMERS
    // 
    private readonly Timer _aiTimer;
    private readonly Timer _msgTimer;
    private readonly Timer _screenTimer;   //s

    // 
    // STATE
    // 
    private bool _aiInitialLoaded = false;
    private bool _screenInitialLoaded = false;

    private double _aiTimeCounter = 0;
    private double _screenTimeCounter = 0;

    private int _canvasIndex = 1;

    public ObservableCollection<string> MessageList { get; } = new();

    // 
    // CONSTRUCTOR
    // 
    public AnalyticsViewModel(IThemeService? themeService = null, IRpcEventService rpcEventService = null!)
    {
        _themeService = themeService;

        if (_themeService != null)
        {
            _themeService.ThemeChanged += OnThemeChanged;
        }

        if (rpcEventService != null)
        {
            System.Diagnostics.Debug.WriteLine("RPC event service registeration happens");
        }

        // AI Graph Timer (4s)
        _aiTimer = new Timer(4000);
        _aiTimer.Elapsed += async (_, _) => await UpdateAI();
        _aiTimer.Start();
        _ = UpdateAI();

        // Message Timer (5s)
        _msgTimer = new Timer(5000);
        _msgTimer.Elapsed += (_, _) => AddMessages();
        _msgTimer.Start();

        // ScreenShare Timer (7s)
        _screenTimer = new Timer(7000);
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
        Application.Current.Dispatcher.Invoke(() => ApplyTheme());
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
    // GRAPH 1 — AI SENTIMENT (Dynamic)
    //
    private async Task UpdateAI()
    {
        if (!_aiInitialLoaded)
        {
            List<AIData> initialData = await _aiService.FetchAIDataAsync();

            foreach (AIData d in initialData)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    Graph1_AI.AddPoint(_aiTimeCounter, d.Value);
                    _aiTimeCounter += 5;
                });
            }

            _aiInitialLoaded = true;
            return;
        }

        // Add random new point
        var rand = new Random();
        double newValue = rand.Next(-5, 10);

        Application.Current.Dispatcher.Invoke(() => {
            Graph1_AI.Add(_aiTimeCounter, newValue);
        });

        _aiTimeCounter += 4;
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
    // GRAPH 3 — AI Messages
    //
    private void AddMessages()
    {
        List<AIMessageData> messages = _msgService.GetNext();

        Application.Current.Dispatcher.Invoke(() => {
            foreach (AIMessageData m in messages)
            {
                MessageList.Add(m.Message);
            }
        });
    }

    // 
    // GRAPH 4 — ScreenShare Sentiment (NEW)
    // 
    private async Task UpdateScreenShare()
    {
        // FIRST LOAD
        if (!_screenInitialLoaded)
        {
            List<ScreenShareData> initial = await _screenService.ScreenShareDatasAsync();

            foreach (ScreenShareData d in initial)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    Graph4_Screen.AddPoint(_screenTimeCounter, d.Sentiment);
                    _screenTimeCounter += 10;
                });
            }

            _screenInitialLoaded = true;
            return;
        }

        //  add random new screenshare sentiment
        var rand = new Random();
        double newSentiment = rand.Next(-5, 10);

        Application.Current.Dispatcher.Invoke(() => {
            Graph4_Screen.Add(_screenTimeCounter, newSentiment);
        });

        _screenTimeCounter += 8;
    }
}
