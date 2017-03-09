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
using GuessMyPhoto.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameInfoPage : Page
    {
        private GameInfoViewModel ViewModel;
        public GameInfoPage()
        {
            this.InitializeComponent();
        }

        private async void PlayerListBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;
            ProgRing.IsActive = true;
            await ViewModel.GoToPlayersList();
            ProgRing.IsActive = true;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ViewModel = e.Parameter as GameInfoViewModel;
            }
            if (ViewModel == null)
                Frame.GoBack();
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(!await ViewModel.Initialize())
                Frame.GoBack();
        }

        private async void ChallangeBtn_Click(object sender, RoutedEventArgs e)
        {
            EndGameGrid.Margin = new Thickness(EndGameGrid.Margin.Left, EndGameGrid.Margin.Top,
                        EndGameGrid.Margin.Right, 20);
            ProgRing.IsActive = true;
            await ViewModel.GoToShareOrChallenge();
            ProgRing.IsActive = false;
        }

        private void BackChallengekBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.HideChallengeControl();
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsGridView.SelectedItems.Count > 0)
                SendChallengeBtn.IsEnabled = true;
            else
                SendChallengeBtn.IsEnabled = false;

            if (ContactsGridView.SelectedItems.Count > 20)
            {
                ContactsGridView.SelectedItems.Remove(e.AddedItems.LastOrDefault());
            }
        }

        private async void SendChallengeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;

            ProgRing.IsActive = true;
            SendChallengeBtn.IsEnabled = false;
            BackChallengekBtn.IsEnabled = false;
            var result = await ViewModel.SendChallenges(ContactsGridView.SelectedItems);
            ProgRing.IsActive = false;
            SendChallengeBtn.IsEnabled = true;
            BackChallengekBtn.IsEnabled = true;
            if (result == null || result.Status != CreateChallengeStatus.Ok)
            {
                var dialog = new MessageDialog("An error occurred when trying to create challende(s). Please try again later.");
                await dialog.ShowAsync();
            }
            else
            {
                var dialog = new MessageDialog("Challenge created successfully");
                await dialog.ShowAsync();
                //ViewModel.HideChallengeControl();
                Frame.GoBack();
            }
        }

        private void BackgoundImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!ViewModel.NeedDisplayPlayerListControl && !ViewModel.NeedDisplayCallengeControl)
            {
                if (Math.Abs(EndGameGrid.Margin.Bottom - 20) < 0.01)
                    EndGameGrid.Margin = new Thickness(EndGameGrid.Margin.Left, EndGameGrid.Margin.Top,
                        EndGameGrid.Margin.Right, -80);
                else
                    EndGameGrid.Margin = new Thickness(EndGameGrid.Margin.Left, EndGameGrid.Margin.Top,
                        EndGameGrid.Margin.Right, 20);
            }
        }

        private void GamePhotoImg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GamePhotoImg.ActualHeight > GamePhotoImg.ActualWidth)
                GamePhotoImg.Height = GamePhotoImg.ActualWidth;
            else
                GamePhotoImg.Width = GamePhotoImg.ActualHeight;
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.HidePlayersListControl();
        }
    }
}
