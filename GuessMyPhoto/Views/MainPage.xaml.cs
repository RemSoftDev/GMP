using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GuessMyPhoto.Models.Game;
using GuessMyPhoto.Models.User;
using GuessMyPhoto.ViewModels;
using GuessMyPhoto.Models;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Views.ContentDialogs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel ViewModel;
        private Thickness margin;
        private double maxScrollOfset = 0;
        public MainPage()
        {
            this.InitializeComponent();

            margin = HeaderScrollViewer.Margin;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = new MainPageViewModel();
            base.OnNavigatedTo(e);
            var pageType = Frame.BackStack.Last().SourcePageType;
            if (pageType == typeof(CreateUserPage) || pageType == typeof(StartPage))
                App.CanGoBack = false;

            
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.CanGoBack = true;
        }
        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //Picture picture = new Picture();
            //StorageFile file = await picture.PickImage();
            //string binImage = await picture.GetPictureBytes(file);
            //User user = new User();
            //await user.UploadProfilePhoto("9b72c4317bd4f4808714856a5c18110c", binImage);
        }

        private async void AddNewGame_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SelectPictureSourceDialog(false);
            await dialog.ShowAsync();
            if (dialog.Result == DialogResultEnum.Accept)
            {
                Frame.Navigate(typeof(AddNewGamePage), new AddNewGameViewModel(dialog.SelectedPicrureStore));
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AddNewGame_Click(sender, null);
        }

        private void MyProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyProfilePage));
        }

        private void MyActivityBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ActivityPage), new ActivityViewModel(ViewModel.UserPhoto));
        }

        private void GamesScrollViewer_ViewChanging_1(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (-e.NextView.VerticalOffset < -165)
                margin.Top = -165;
            else
                margin.Top = -e.NextView.VerticalOffset;
            HeaderScrollViewer.Margin = margin;
        }

        private void ListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListView;
            var item = listViewItem?.SelectedItem as Puzzle;
            if (item != null)
                Frame.Navigate(typeof(GamePage), new GameViewModel(item));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.UpdatePuzzlesList();
        }

        private async void RadioButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            maxScrollOfset = 0;
            await ViewModel.UpdatePuzzlesList();
        }

        private void GamesScrollViewer_DirectManipulationCompleted(object sender, object e)
        {
            if (GamesScrollViewer.VerticalOffset > 900)
            {
                if (maxScrollOfset < GamesScrollViewer.VerticalOffset)
                {
                    maxScrollOfset = GamesScrollViewer.VerticalOffset;
                    ViewModel.LoadImage(((int) maxScrollOfset/105) + 5);
                }
            }
        }
    }
}
