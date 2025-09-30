using System.Windows.Controls;
using GUI.ViewModels.Auth;

namespace GUI.Views.Auth
{
    /// <summary>
    /// Presents the login form and synchronizes secure password entry with its view model.
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

        /// <summary>
        /// Mirrors the masked password input into the bound <see cref="LoginViewModel"/>.
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}