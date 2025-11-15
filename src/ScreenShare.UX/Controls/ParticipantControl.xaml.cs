using System.Windows;
using System.Windows.Controls;

namespace ScreenShare.UX.Controls
{
    /// <summary>
    /// Interaction logic for ParticipantControl.xaml
    /// </summary>
    public partial class ParticipantControl : UserControl
    {
        public static readonly DependencyProperty InitialProperty =
            DependencyProperty.Register("Initial", typeof(string), typeof(ParticipantControl), 
                new PropertyMetadata("Y", OnInitialChanged));

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(ParticipantControl), 
                new PropertyMetadata("You", OnUsernameChanged));

        public string Initial
        {
            get { return (string)GetValue(InitialProperty); }
            set { SetValue(InitialProperty, value); }
        }

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public ParticipantControl()
        {
            InitializeComponent();
        }

        private static void OnInitialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticipantControl;
            if (control != null)
            {
                control.InitialTextBlock.Text = e.NewValue?.ToString() ?? "Y";
            }
        }

        private static void OnUsernameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticipantControl;
            if (control != null)
            {
                control.UsernameTextBlock.Text = e.NewValue?.ToString() ?? "You";
            }
        }
    }
}
