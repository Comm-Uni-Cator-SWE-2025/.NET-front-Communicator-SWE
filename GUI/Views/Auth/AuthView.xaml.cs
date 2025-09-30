using System.Windows.Controls;

namespace GUI.Views.Auth
{
    /// <summary>
    /// Hosts the authentication workflow shell, swapping between login and sign-up content.
    /// </summary>
    public partial class AuthView : UserControl
    {
        /// <summary>
        /// Initializes UI components generated from XAML.
        /// </summary>
        public AuthView()
        {
            InitializeComponent();
        }
    }
}
