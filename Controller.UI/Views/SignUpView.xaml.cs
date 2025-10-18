using System.Windows.Controls;

namespace Controller.UI.Views
{
    /// <summary>
    /// Collects registration details using attached behavior for password binding (pure MVVM).
    /// </summary>
    public partial class SignUpView : UserControl
    {
        /// <summary>
        /// Initializes XAML components for the sign-up page.
        /// </summary>
        public SignUpView()
        {
            InitializeComponent();
        }
    }
}
