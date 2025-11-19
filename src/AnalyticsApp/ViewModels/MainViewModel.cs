//using AnalyticsApp.Models;
//using AnalyticsApp.Services;
//using LiveChartsCore.Defaults;
//using System.Timers;

//namespace AnalyticsApp.ViewModels;

///// <summary>
///// ViewModel for the main analytics page.
///// Controls 4 graphs:
/////  - Graph1: AI Data (active)
/////  - Graph2: Canvas Data (disabled for now)
/////  - Graph3: AI Message Analytics (disabled for now)
/////  - Graph4: Screen Share Analytics (disabled for now)
///// 
///// This ViewModel fetches live AI data every 4 seconds
///// and appends new points to Graph1.
///// </summary>
//public class MainPageViewModel
//{
//    /// <summary>
//    /// Graph 1: Displays AI Data (sentiment/time).
//    /// </summary>
//    public GraphViewModel Graph1_AI { get; } = new();

//    /// <summary>
//    /// Graph 2: Canvas analytics (not active yet).
//    /// </summary>
//    public GraphViewModel Graph2_Canvas { get; } = new();

//    /// <summary>
//    /// Graph 3: AI message analytics (not active yet).
//    /// </summary>
//    public GraphViewModel Graph3_Msg { get; } = new();

//    /// <summary>
//    /// Graph 4: Screen share analytics (not active yet).
//    /// </summary>
//    public GraphViewModel Graph4_Screen { get; } = new();

//    // 
//    // Services
//    // 

//    /// <summary>
//    /// Service that fetches AI data from API.
//    /// </summary>
//    private readonly ApiService _aiService = new();

//    // Future expansion:
//    // private readonly CanvasService _canvasService = new();
//    // private readonly AIMessageService _msgService = new();
//    // private readonly ScreenShareService _screenService = new();

//    /// <summary>
//    /// Timer that triggers every 4 seconds.
//    /// </summary>
//    private readonly System.Timers.Timer _timer;

//    /// <summary>
//    /// Converts X-axis numeric double → readable "HH:mm:ss" label.
//    /// </summary>
//    public Func<double, string> TimeFormatter =>
//        v => DateTime.FromOADate(v).ToString("HH:mm:ss");
//    private bool _initialLoaded = false;
//    /// <summary>
//    /// Creates the main ViewModel and starts auto-updating.
//    /// </summary>
//    public MainPageViewModel()
//    {
//        // Timer: trigger every 4 seconds
//        _timer = new System.Timers.Timer(4000);
//        _timer.Elapsed += async (_, _) => await UpdateAI();
//        _timer.Start();
//        _ = UpdateAI();
//        // Load once immediately on startup
//        _ = UpdateAllGraphs();
//    }

//    /// <summary>
//    /// Fetches all graph data (currently only AI Data is enabled).
//    /// </summary>
//    private async Task UpdateAllGraphs()
//    {
//        // 
//        // 1. AI DATA UPDATE (Graph 1)
//        //
//        var aiData = await _aiService.FetchAIDataAsync();

//        foreach (var d in aiData)
//            Add(Graph1_AI, d.Time, d.Value);


//        // 
//        // 2. Other graphs (future use)
//        // 

//        // var canvas = await _canvasService.GetCanvasDataAsync();
//        // Add(Graph2_Canvas, canvas.Time, canvas.Value);

//        // var msg = await _msgService.GetMessageDataAsync();
//        // Add(Graph3_Msg, msg.Time, msg.Count);

//        // var screen = await _screenService.GetScreenDataAsync();
//        // Add(Graph4_Screen, screen.Time, screen.Activity);
//    }
//    /// <summary>
//    /// Loads initial API data ONCE, then adds random new points afterward.
//    /// </summary>
//    private async Task UpdateAI()
//    {
//        // 1️⃣ FIRST TIME → LOAD API DATA
//        if (!_initialLoaded)
//        {
//            var initialData = await _aiService.FetchAIDataAsync();

//            foreach (var d in initialData)
//            {
//                App.Current.Dispatcher.Invoke(() =>
//                {
//                    Graph1_AI.Points.Add(new DateTimePoint(d.Time, d.Value));
//                });
//            }

//            _initialLoaded = true;
//            return;
//        }
//        // 2️⃣ AFTER FIRST TIME → ADD RANDOM NEW DATA
//        var lastTime = Graph1_AI.Points.Last().DateTime;
//        var newTime = lastTime.AddMinutes(1); // Keep incremental time (+1 minute each update)

//        var random = new Random();
//        double newValue = random.Next(-5, 10); // random AI value

//        App.Current.Dispatcher.Invoke(() =>
//        {
//            Graph1_AI.Points.Add(new DateTimePoint(newTime, newValue));
//        });
//    }


//    /// <summary>
//    /// Adds a new point to a graph.
//    /// </summary>
//    private void Add(GraphViewModel graph, DateTime time, double value)
//    {
//        graph.Points.Add(new DateTimePoint(time, value));
//    }
//}

using AnalyticsApp.Models;
using AnalyticsApp.Services;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AnalyticsApp.ViewModels;

/// <summary>
/// Main ViewModel controlling all 4 graphs:
///  - Graph1: AI Sentiment Time Series (dynamic)
///  - Graph2: Canvas Shape Count (last 3 snapshots)
///  - Graph3: AI Messages (text list)
///  - Graph4: Reserved for screen share analytics
/// </summary>
public class MainPageViewModel
{
    // 
    // Graphs
    // 
    public GraphViewModel Graph1_AI { get; } = new();
    public CanvasGraphViewModel Graph2_Canvas { get; } = new(); // ⭐ NEW
    public GraphViewModel Graph3_Msg { get; } = new();
    public GraphViewModel Graph4_Screen { get; } = new();

    // 
    // Services
    // 
    private readonly ApiService _aiService = new();
    private readonly AIMessageService _msgService = new();
    private readonly CanvasDataService _canvasService = new(); // ⭐ NEW

    // 
    // Timers
    // 
    private readonly System.Timers.Timer _aiTimer;
    private readonly Timer _msgTimer;
    private readonly Timer _canvasTimer; // ⭐ NEW

    // 
    // State
    // 
    private bool _initialLoaded = false;
    private double _timeCounter = 0;
    private int _canvasIndex = 1;

    /// <summary>
    /// Stores AI messages for Graph3 ListBox
    /// </summary>
    public ObservableCollection<string> MessageList { get; } = new();

    // 
    // CONSTRUCTOR
    //
    public MainPageViewModel()
    {
        // AI Graph Timer (every 4 seconds)
        _aiTimer = new Timer(4000);
        _aiTimer.Elapsed += async (_, _) => await UpdateAI();
        _aiTimer.Start();
        _ = UpdateAI();

        // AI Message Timer (every 5 seconds)
        _msgTimer = new Timer(5000);
        _msgTimer.Elapsed += (_, _) => AddMessages();
        _msgTimer.Start();

        // Canvas Graph Timer (every 6 seconds)
        _canvasTimer = new Timer(6000);
        _canvasTimer.Elapsed += (_, _) => UpdateCanvas();
        _canvasTimer.Start();
    }

    // 
    // GRAPH 1 -> AI SENTIMENT
    //
    private async Task UpdateAI()
    {
        // FIRST LOAD: Load initial API data
        if (!_initialLoaded)
        {
            var initialData = await _aiService.FetchAIDataAsync();

            foreach (var d in initialData)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Graph1_AI.AddPoint(_timeCounter, d.Value);
                    _timeCounter += 5;
                });
            }

            _initialLoaded = true;
            return;
        }

        // AFTER FIRST TIME -> Add random new point
        var random = new Random();
        double newValue = random.Next(-5, 10);

        App.Current.Dispatcher.Invoke(() =>
        {
            Graph1_AI.Add(_timeCounter, newValue);
        });

        _timeCounter += 4;
    }

    // 
    //GRAPH 2 -> CANVAS SHAPE CHART
    //
    private void UpdateCanvas()
    {
        var data = _canvasService.FetchNext();

        App.Current.Dispatcher.Invoke(() =>
        {
            Graph2_Canvas.AddSnapshot(data, $"T{_canvasIndex}");
            _canvasIndex++;
        });
    }

    //
    // GRAPH 3 -> AI MESSAGE LIST
    //
    private void AddMessages()
    {
        var messages = _msgService.GetNext();

        App.Current.Dispatcher.Invoke(() =>
        {
            foreach (var msg in messages)
                MessageList.Add(msg.Message);
        });
    }

    //
    // GRAPH 4 -> SCREENSHARE 
}



