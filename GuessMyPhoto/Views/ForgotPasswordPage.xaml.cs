using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.User;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var email = e.Parameter as string;
            if (!string.IsNullOrWhiteSpace(email))
                EmailTextBox.Text = email;
            base.OnNavigatedTo(e);
        }
        private async void RestorePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                FlyoutTextBlock.Text = "You must enter your Email!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                return;
            }
            if (!Regex.IsMatch(EmailTextBox.Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                FlyoutTextBlock.Text = "The email is invalid!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                return;
            }

            if (!await LogicHelper.IsInternet())
                return;

            ProgRing.IsActive = true;
            EmailTextBox.IsEnabled = false;
            RestorePasswordBtn.IsEnabled = false;
            var result = await User.ForgotPassword(EmailTextBox.Text);
            var dialog = new MessageDialog("","");
            if (result == null || result.Status != UserForgotPasswordStatus.Ok)
            {
                dialog.Content = "Failed to restore a passwotd. Please check the Email and internet connection.";
            }
            else if (result.Status == UserForgotPasswordStatus.Ok)
            {
                dialog.Content = "An email with instructions was sent to your email-address.";
            }
            await dialog.ShowAsync();
            ProgRing.IsActive = false;
            EmailTextBox.IsEnabled = true;
            RestorePasswordBtn.IsEnabled = true;
            if (result?.Status == UserForgotPasswordStatus.Ok)
                Frame.GoBack();
        }
    }
}
