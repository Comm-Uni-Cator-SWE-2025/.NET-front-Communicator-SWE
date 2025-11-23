using System.Collections.ObjectModel;
using System.Timers;
using AnalyticsApp.Models;
using AnalyticsApp.Services;
using LiveChartsCore.Defaults;
using Timer = System.Timers.Timer;

namespace AnalyticsApp.ViewModels;

public class MainPageViewModel
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

    // 
    // TIMERS
    // 
    private readonly Timer _aiTimer;
    private readonly Timer _msgTimer;
    private readonly Timer _canvasTimer;
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
    public MainPageViewModel()
    {
        // AI Graph Timer (4s)
        _aiTimer = new Timer(4000);
        _aiTimer.Elapsed += async (_, _) => await UpdateAI();
        _aiTimer.Start();
        _ = UpdateAI();

        // Message Timer (5s)
        _msgTimer = new Timer(5000);
        _msgTimer.Elapsed += (_, _) => AddMessages();
        _msgTimer.Start();

        // Canvas Timer (6s)
        _canvasTimer = new Timer(6000);
        _canvasTimer.Elapsed += (_, _) => UpdateCanvas();
        _canvasTimer.Start();

        // ScreenShare Timer (7s)
        _screenTimer = new Timer(7000);
        _screenTimer.Elapsed += async (_, _) => await UpdateScreenShare();
        _screenTimer.Start();
        _ = UpdateScreenShare();
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
                App.Current.Dispatcher.Invoke(() =>
                {
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

        App.Current.Dispatcher.Invoke(() =>
        {
            Graph1_AI.Add(_aiTimeCounter, newValue);
        });

        _aiTimeCounter += 4;
    }

    //
    // GRAPH 2 — Canvas Snapshot
    //
    private void UpdateCanvas()
    {
        CanvasData data = _canvasService.FetchNext();

        App.Current.Dispatcher.Invoke(() =>
        {
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

        App.Current.Dispatcher.Invoke(() =>
        {
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
                App.Current.Dispatcher.Invoke(() =>
                {
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

        App.Current.Dispatcher.Invoke(() =>
        {
            Graph4_Screen.Add(_screenTimeCounter, newSentiment);
        });

        _screenTimeCounter += 8;
    }
}
