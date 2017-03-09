using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using GuessMyPhoto.Models;
using GuessMyPhoto.Models.Game;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            this.InitializeComponent();

        }

        private async void LoginWithFBBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;
            ButtonsGrid.Visibility = Visibility.Collapsed;
            ProgRing.IsActive = true;
            UserFbLoginResponse loginResult = await User.LoginFb();
            ProgRing.IsActive = false;
            ButtonsGrid.Visibility = Visibility.Visible;
            if (loginResult?.Status == FbLoginStatus.Ok)
                Frame.Navigate(typeof(MainPage));
            else
            {
                MessageDialog dialog = new MessageDialog("Failed connect with Facebook!", "Can't connect");
                await dialog.ShowAsync();
            }
        }

        private void CreateUserBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (CreateUserPage));
        }

        private void LogInBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            NoInternetStackPanel.Visibility = Visibility.Collapsed;
            //bool connection = await LogicHelper.CheckInternetConnection();
            if (await LogicHelper.IsInternet())
            {
                var user = await AppInfo.InitializeUser();
                ProgRing.IsActive = false;
                if (user == null)
                    ButtonsGrid.Visibility = Visibility.Visible;
                else
                    Frame.Navigate(typeof(MainPage));
            }
            else
            {
                NoInternetStackPanel.Visibility = Visibility.Visible;
                ProgRing.IsActive = false;
            }
        }

        private void TryAgainBtn_Click(object sender, RoutedEventArgs e)
        {
            Page_Loaded(null,null);
        }
    }
}
