/*
 * -----------------------------------------------------------------------------
 *  File: HomePageView.xaml.cs
 *  Owner: Pramodh Sai
 *  Roll Number : 112201029
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Communicator.App.Views.Home;

/// <summary>
/// Landing page shown after authentication, presenting meeting shortcuts and user context.
/// </summary>
public sealed partial class HomePageView : UserControl
{
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// Initializes the home page UI defined in XAML.
    /// </summary>
    public HomePageView()
    {
        InitializeComponent();

        _timer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        Loaded += (s, e) => _timer.Start();
        Unloaded += (s, e) => _timer.Stop();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        DateTime now = DateTime.Now;

        if (SecondHandTransform != null)
        {
            SecondHandTransform.Angle = now.Second * 6;
        }

        if (MinuteHandTransform != null)
        {
            MinuteHandTransform.Angle = (now.Minute * 6) + (now.Second * 0.1);
        }

        if (HourHandTransform != null)
        {
            HourHandTransform.Angle = (now.Hour * 30) + (now.Minute * 0.5);
        }
    }
}



