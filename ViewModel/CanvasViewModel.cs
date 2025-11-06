using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;
using CanvasDataModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace ViewModel;

public class CanvasViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public enum DrawingMode { FreeHand, StraightLine, Rectangle, EllipseShape, TriangleShape }
    private DrawingMode _currentMode = DrawingMode.FreeHand;
    public DrawingMode CurrentMode
    {
        get => _currentMode;
        set => _currentMode = value;
    }
    private List<Point> _trackedPoints = new(); // this is for tracking mouse movements
    public bool _isTracking = false;

    public ObservableCollection<IShape> _shapes = new();  // current canvas shapes
    private readonly StateManager _stateManager = new();     // current canvas shapes
    public Color CurrentColor { get; set; } = Color.Red;
    public string CurrentUserId { get; set; } = "user_default"; // <-- ADD THIS (mock ID for now)
    private double _currentThickness = 2.0; // Default value
    public double CurrentThickness
    {
        get => _currentThickness;
        set
        {
            if (_currentThickness != value)
            {
                _currentThickness = value;
                OnPropertyChanged(); // <-- This notifies the UI (the slider)
            }
        }
    }
    public void StartTracking(Point point)
    {
        _isTracking = true;
        if (CurrentMode == DrawingMode.FreeHand)
        {
            _trackedPoints.Clear();
            _trackedPoints.Add(point);
        }
        else if (CurrentMode == DrawingMode.StraightLine || CurrentMode == DrawingMode.Rectangle || CurrentMode == DrawingMode.EllipseShape || CurrentMode == DrawingMode.TriangleShape)
        {
            _trackedPoints.Clear();
            _trackedPoints.Add(point); // start point
            _trackedPoints.Add(point); // end point (will be updated on mouse move)
        }
    }
    public void TrackPoint(Point point)
    {
        if (_isTracking && _trackedPoints.Count > 0)
        {
            if (CurrentMode == DrawingMode.FreeHand)
            {
                _trackedPoints.Add(point);
            }
            else if (CurrentMode == DrawingMode.StraightLine || CurrentMode == DrawingMode.Rectangle || CurrentMode == DrawingMode.EllipseShape || CurrentMode == DrawingMode.TriangleShape)
            {
                _trackedPoints[1] = point; // update end point
            }
        }
    }

    public void StopTracking()
    {
        _isTracking = false;
        if (_trackedPoints.Count == 0)
        {
            return;
        }
        if (CurrentMode == DrawingMode.FreeHand)
        {
            if (_trackedPoints.Count == 0)
            {
                return;
            }
            //// ----- 2. ADDED THIS LOGGING CODE -----
            //// Format the points as a string: [(x1,y1), (x2,y2), ...]
            //var pointsString = string.Join(", ", _trackedPoints.Select(p => $"({p.X},{p.Y})"));
            //Debug.WriteLine($"New FreeHand Shape with {_trackedPoints.Count} points: [{pointsString}]");
            //// ------------------------------------
            var freehand = new FreeHand(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            _shapes.Add(freehand);
            _stateManager.AddState(new State(_shapes));
            return;
        }
        else if (CurrentMode == DrawingMode.StraightLine)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            //points generate
            var line = new StraightLine(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            _shapes.Add(line);
            _stateManager.AddState(new State(_shapes));
            return;
        }
        else if (CurrentMode == DrawingMode.Rectangle)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var rectangle = new RectangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            _shapes.Add(rectangle);
            _stateManager.AddState(new State(_shapes));
            return;
        }
        else if (CurrentMode == DrawingMode.EllipseShape)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var ellipse = new EllipseShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            _shapes.Add(ellipse);
            _stateManager.AddState(new State(_shapes));
            return;
        }
        else if (CurrentMode == DrawingMode.TriangleShape)
        {
            if (_trackedPoints.Count < 2)
            {
                return;
            }
            var ellipse = new TriangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
            _shapes.Add(ellipse);
            _stateManager.AddState(new State(_shapes));
            return;
        }
    }
    public IShape? CurrentPreviewShape
    {
        get
        {
            if (!_isTracking || _trackedPoints.Count < 2)
            {
                return null;
            }
            switch (CurrentMode)
            {
                case DrawingMode.FreeHand:
                    return new FreeHand(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.StraightLine:
                    return new StraightLine(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.Rectangle:
                    return new RectangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.EllipseShape:
                    return new EllipseShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                case DrawingMode.TriangleShape:
                    return new TriangleShape(_trackedPoints, CurrentColor, CurrentThickness, CurrentUserId);
                default:
                    return null;
            }
        }
    }

    public void Undo()
    {
        State prev = _stateManager.Undo(); // StateManager is in Model
        if (prev != null)
        {
            _shapes.Clear();
            foreach (IShape s in prev.Shapes) { _shapes.Add(s); }
        }
    }

    public void Redo()
    {
        State next = _stateManager.Redo(); // StateManager is in Model
        if (next != null)
        {
            _shapes.Clear();
            foreach (IShape s in next.Shapes) { _shapes.Add(s); }
        }
    }

    public void AddTestShape()
    {
        //1.Define your list of points here
        List<Point> testPoints = new List<Point>
        {
            new Point( 258,  217),
            new Point( 403, 354 )
        };

        // 2. Decide what kind of shape to create
        //    Let's create a FreeHand shape using a different color
        var testShape = new EllipseShape(testPoints, Color.Purple, CurrentThickness, CurrentUserId);

        // 3. Add it to the main shapes list
        _shapes.Add(testShape);

        // 4. Save the new state for Undo/Redo
        _stateManager.AddState(new State(_shapes));

        List<Point> testPoints1 = new List<Point>
        {
            new Point(357,  221),new Point(356,  221),new Point(355,  221),new Point(354,  221),new Point(353,  220),new Point(352,  220),new Point(351,  220),new Point(350,  220),new Point(349,  220),new Point(348,  220),new Point(347,  220),new Point(346,  220),new Point(345,  220),new Point(344,  220),new Point(343,  220),new Point(342,  220),new Point(341,  220),new Point(340,  220),new Point(339,  220),new Point(338,  220),new Point(337,  220),new Point(336,  220),new Point(335,  220),new Point(334,  220),new Point(333,  220),new Point(332,  220),new Point(331,  220),new Point(330,  220),new Point(329,  220),new Point(328,  220),new Point(327,  220),new Point(326,  220),new Point(325,  220),new Point(324,  220),new Point(323,  220),new Point(322,  220),new Point(321,  220),new Point(320,  220),new Point(319,  220),new Point(318,  220),new Point(317,  220),new Point(316,  220),new Point(315,  220),new Point(314,  220),new Point(313,  220),new Point(312,  220),new Point(311,  221),new Point(310,  221),new Point(309,  221),new Point(308,  221),new Point(307,  222),new Point(306,  222),new Point(305,  222),new Point(304,  223),new Point(303,  223),new Point(302,  223),new Point(301,  224),new Point(300,  224),new Point(299,  225),new Point(298,  225),new Point(297,  226),new Point(297,  227),new Point(296,  227),new Point(295,  227),new Point(294,  228),new Point(293,  228),new Point(293,  229),new Point(292,  229),new Point(292,  230),new Point(291,  230),new Point(291,  231),new Point(290,  231),new Point(289,  232),new Point(288,  233),new Point(288,  234),new Point(287,  234),new Point(287,  235),new Point(286,  235),new Point(286,  236),new Point(285,  236),new Point(285,  237),new Point(284,  237),new Point(284,  238),new Point(283,  238),new Point(282,  239),new Point(282,  240),new Point(281,  240),new Point(281,  241),new Point(280,  241),new Point(279,  242),new Point(278,  242),new Point(277,  243),new Point(276,  243),new Point(276,  244),new Point(275,  245),new Point(275,  246),new Point(274,  246),new Point(274,  247),new Point(273,  248),new Point(272,  249),new Point(272,  250),new Point(271,  250),new Point(271,  251),new Point(270,  252),new Point(270,  253),new Point(269,  253),new Point(269,  254),new Point(269,  255),new Point(268,  256),new Point(267,  257),new Point(266,  258),new Point(266,  259),new Point(266,  260),new Point(266,  261),new Point(265,  261),new Point(264,  262),new Point(264,  263),new Point(264,  264),new Point(263,  264),new Point(263,  265),new Point(262,  266),new Point(262,  267),new Point(262,  268),new Point(261,  269),new Point(261,  270),new Point(260,  271),new Point(260,  272),new Point(260,  273),new Point(260,  274),new Point(260,  275),new Point(260,  276),new Point(259,  277),new Point(259,  278),new Point(259,  279),new Point(259,  280),new Point(259,  281),new Point(259,  282),new Point(259,  283),new Point(259,  284),new Point(259,  285),new Point(258,  285),new Point(258,  286),new Point(258,  287),new Point(258,  288),new Point(258,  289),new Point(258,  290),new Point(258,  291),new Point(258,  292),new Point(258,  293),new Point(258,  294),new Point(258,  295),new Point(258,  296),new Point(258,  297),new Point(258,  298),new Point(258,  299),new Point(258,  300),new Point(258,  301),new Point(258,  302),new Point(259,  302),new Point(259,  303),new Point(259,  304),new Point(259,  305),new Point(259,  306),new Point(259,  307),new Point(259,  308),new Point(259,  310),new Point(261,  311),new Point(261,  312),new Point(261,  313),new Point(261,  314),new Point(261,  315),new Point(263,  316),new Point(263,  317),new Point(263,  318),new Point(264,  320),new Point(265,  321),new Point(266,  323),new Point(266,  324),new Point(267,  324),new Point(268,  325),new Point(269,  327),new Point(269,  328),new Point(270,  329),new Point(271,  330),new Point(273,  331),new Point(274,  332),new Point(274,  333),new Point(275,  333),new Point(276,  333),new Point(276,  334),new Point(277,  334),new Point(277,  335),new Point(279,  336),new Point(279,  337),new Point(280,  337),new Point(281,  338),new Point(281,  339),new Point(282,  339),new Point(283,  340),new Point(284,  341),new Point(285,  342),new Point(286,  343),new Point(287,  343),new Point(288,  344),new Point(288,  345),new Point(289,  345),new Point(290,  346),new Point(291,  347),new Point(294,  348),new Point(295,  349),new Point(297,  349),new Point(297,  350),new Point(299,  350),new Point(300,  351),new Point(301,  351),new Point(302,  351),new Point(306,  352),new Point(307,  352),new Point(308,  352),new Point(309,  353),new Point(310,  353),new Point(312,  353),new Point(313,  353),new Point(318,  354),new Point(320,  354),new Point(322,  354),new Point(323,  354),new Point(325,  354),new Point(327,  354),new Point(328,  354),new Point(330,  354),new Point(334,  354),new Point(336,  354),new Point(337,  354),new Point(339,  354),new Point(340,  354),new Point(341,  354),new Point(346,  353),new Point(347,  353),new Point(348,  353),new Point(350,  353),new Point(352,  353),new Point(353,  353),new Point(355,  353),new Point(360,  351),new Point(361,  351),new Point(362,  350),new Point(365,  349),new Point(366,  349),new Point(368,  348),new Point(371,  346),new Point(373,  345),new Point(375,  344),new Point(376,  343),new Point(377,  342),new Point(378,  341),new Point(382,  338),new Point(383,  337),new Point(385,  336),new Point(386,  334),new Point(388,  333),new Point(389,  332),new Point(393,  329),new Point(394,  327),new Point(395,  325),new Point(396,  324),new Point(397,  322),new Point(398,  320),new Point(399,  317),new Point(399,  316),new Point(400,  315),new Point(401,  313),new Point(401,  310),new Point(401,  308),new Point(402,  305),new Point(402,  303),new Point(402,  302),new Point(403,  300),new Point(403,  298),new Point(403,  293),new Point(403,  291),new Point(403,  290),new Point(403,  288),new Point(403,  286),new Point(403,  285),new Point(403,  282),new Point(403,  279),new Point(403,  277),new Point(403,  275),new Point(402,  273),new Point(402,  272),new Point(400,  269),new Point(400,  266),new Point(399,  265),new Point(399,  263),new Point(398,  262),new Point(396,  256),new Point(394,  253),new Point(393,  252),new Point(392,  251),new Point(390,  250),new Point(388,  247),new Point(387,  245),new Point(386,  244),new Point(385,  243),new Point(384,  243),new Point(382,  242),new Point(382,  241),new Point(381,  240),new Point(379,  239),new Point(379,  238),new Point(377,  237),new Point(377,  236),new Point(377,  235),new Point(376,  235),new Point(375,  235),new Point(375,  234),new Point(374,  234),new Point(373,  233),new Point(370,  231),new Point(369,  231),new Point(368,  231),new Point(368,  230),new Point(367,  230),new Point(367,  229),new Point(366,  229),new Point(365,  229),new Point(364,  229),new Point(363,  227),new Point(361,  227),new Point(360,  226),new Point(358,  225),new Point(357,  225),new Point(356,  224),new Point(355,  224),new Point(354,  224),new Point(353,  223),new Point(352,  223),new Point(352,  222),new Point(351,  222),new Point(350,  222),new Point(349,  222),new Point(348,  222),new Point(348,  221),new Point(347,  221),new Point(346,  220),new Point(345,  220),new Point(344,  220),new Point(344,  219),new Point(343,  219),new Point(342,  219),new Point(340,  218),new Point(339,  218),new Point(338,  218),new Point(337,  218),new Point(337,  217)
        };
        var testShape1 = new FreeHand(testPoints1, Color.Red, CurrentThickness, CurrentUserId);
        _shapes.Add(testShape1);

        //4.Save the new state for Undo / Redo

        _stateManager.AddState(new State(_shapes));

    }

}
