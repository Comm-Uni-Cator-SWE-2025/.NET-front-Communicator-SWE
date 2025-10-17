using System.Windows.Controls;
using Controller.UI.ViewModels;

namespace Controller.UI.Views
{
    /// <summary>
    /// Collects registration details and keeps secure password boxes synchronized with the <see cref="SignUpViewModel"/>.
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

        /// <summary>
        /// Copies the entered password into the view model for validation.
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        /// <summary>
        /// Copies the confirmation password into the view model to compare against the primary password.
        /// </summary>
        private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
