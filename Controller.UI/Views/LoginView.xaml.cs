using System.Windows.Controls;

namespace Controller.UI.Views
{
    /// <summary>
    /// Presents the login form using attached behavior for password binding (pure MVVM).
    /// </summary>
    public partial class LoginView : UserControl
    {
        /// <summary>
        /// Initializes the view components defined in XAML.
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
        }
    }
}
