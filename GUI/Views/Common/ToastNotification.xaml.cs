using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GUI.Models;

namespace GUI.Views
{
    /// <summary>
    /// Visual container for toast messages, handling styling, animations, and lifetime management.
    /// </summary>
    public partial class ToastNotification : UserControl
    {
        private DispatcherTimer? _timer;
        public event EventHandler? CloseRequested;

        /// <summary>
        /// Initializes UI elements wired from XAML markup.
        /// </summary>
        public ToastNotification()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Applies toast metadata, selects styles/icons, and starts the auto-dismiss timer when required.
        /// </summary>
        public void SetToast(ToastMessage toast)
        {
            DataContext = toast;
            
            // Set icon and style based on type
            switch (toast.Type)
            {
                case ToastType.Success:
                    ToastBorder.Style = (Style)Resources["SuccessToast"];
                    IconText.Text = "\uE73E"; // Checkmark
                    break;
                case ToastType.Error:
                    ToastBorder.Style = (Style)Resources["ErrorToast"];
                    IconText.Text = "\uE783"; // Error
                    break;
                case ToastType.Warning:
                    ToastBorder.Style = (Style)Resources["WarningToast"];
                    IconText.Text = "\uE7BA"; // Warning
                    break;
                case ToastType.Info:
                    ToastBorder.Style = (Style)Resources["InfoToast"];
                    IconText.Text = "\uE946"; // Info
                    break;
            }

            // Start timer for auto-dismiss
            if (toast.Duration > 0)
            {
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(toast.Duration)
                };
                _timer.Tick += (s, e) =>
                {
                    _timer.Stop();
                    AnimateOut();
                };
                _timer.Start();
            }
        }

        /// <summary>
        /// Begins the entrance animation once the control is visible.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AnimateIn();
        }

        /// <summary>
        /// Slides and fades the toast into view.
        /// </summary>
        private void AnimateIn()
        {
            var slideIn = new DoubleAnimation
            {
                From = 50,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var translateTransform = new System.Windows.Media.TranslateTransform();
            RenderTransform = translateTransform;

            translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
            BeginAnimation(OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Slides and fades the toast out, notifying listeners when complete.
        /// </summary>
        private void AnimateOut()
        {
            var slideOut = new DoubleAnimation
            {
                To = 50,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            slideOut.Completed += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

            var translateTransform = RenderTransform as System.Windows.Media.TranslateTransform;
            if (translateTransform != null)
            {
                translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
            }
            BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Immediately dismisses the toast when the close button is clicked.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            AnimateOut();
        }
    }
}
