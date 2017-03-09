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
    public sealed partial class LoginPage : Page
    {
        private TextBox notValidTextBox = null;
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            notValidTextBox = null;
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                FlyoutTextBlock.Text = "You must enter your Email!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                notValidTextBox = EmailTextBox;
                return;
            }
            if (!Regex.IsMatch(EmailTextBox.Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                FlyoutTextBlock.Text = "The email is invalid!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                notValidTextBox = EmailTextBox;
                return;
            }
            if (string.IsNullOrWhiteSpace(PassTextBox.Password))
            {
                FlyoutTextBlock.Text = "You must enter a password!";
                comboBoxEmptyFlyout.ShowAt(PassTextBox);
                return;
            }
            if (!await LogicHelper.IsInternet())
                return;

            LoginBtn.IsEnabled = false;
            ForgotPassBtn.IsEnabled = false;
            EmailTextBox.IsEnabled = false;
            PassTextBox.IsEnabled = false;
            ProgRing.IsActive = true;
            var result = await User.LoginNative(EmailTextBox.Text, PassTextBox.Password);
            if (result?.Status == NativeLoginStatus.Ok)
            {
                await AppInfo.InitializeUser();
                if (AppInfo.AppUser != null)
                    Frame.Navigate(typeof(MainPage));
            }
            ProgRing.IsActive = false;
            LoginBtn.IsEnabled = true;
            ForgotPassBtn.IsEnabled = true;
            EmailTextBox.IsEnabled = true;
            PassTextBox.IsEnabled = true;
            if (result == null)
            {
                var dialog = new MessageDialog("Can't get data from server. Please try again later.", "Server error");
                await dialog.ShowAsync();
                return;
            }
            if (result.Status == NativeLoginStatus.NoUser || result.Status == NativeLoginStatus.LoginOrPasswordError)
            {
                var dialog = new MessageDialog("Email or password is incorrect!", "Failed to login");
                await dialog.ShowAsync();
            }
        }

        private void ForgotPassBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ForgotPasswordPage), EmailTextBox.Text);
        }
        private void comboBoxEmptyFlyout_Closed(object sender, object e)
        {
            notValidTextBox?.Focus(FocusState.Programmatic);
        }
    }
}
