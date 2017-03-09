using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Game;
using GuessMyPhoto.ViewModels;
using GuessMyPhoto.Views.ContentDialogs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActivityPage : Page
    {
        private double maxScrollOfset = 0;

        private ActivityViewModel ViewModel;
        public ActivityPage()
        {
            this.InitializeComponent();
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ViewModel = e.Parameter as ActivityViewModel;
            }
            if (ViewModel == null)
                Frame.GoBack();
            base.OnNavigatedTo(e);
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.UpdatePuzzlesList();
        }

        private async void AddNewPuzzle_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SelectPictureSourceDialog(false);
            await dialog.ShowAsync();
            if (dialog.Result == DialogResultEnum.Accept)
            {
                Frame.Navigate(typeof(AddNewGamePage), new AddNewGameViewModel(dialog.SelectedPicrureStore));
            }
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
                    ViewModel.LoadImage(((int)maxScrollOfset / 105) + 5);
                }
            }
        }

        private void GamesListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListView;
            var item = listViewItem?.SelectedItem as Puzzle;
            if (item != null)
                Frame.Navigate(typeof(GameInfoPage), new GameInfoViewModel(item, ViewModel.CreatedSelected.HasValue && ViewModel.CreatedSelected.Value, ViewModel.ChallengesSelected.HasValue && ViewModel.ChallengesSelected.Value));
        }
    }
}
