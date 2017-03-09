using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Contacts;
using GuessMyPhoto.Models.Game;
using GuessMyPhoto.ViewModels;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        private GameViewModel ViewModel;
        public GamePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ViewModel = e.Parameter as GameViewModel;
            }
            if (ViewModel == null)
                Frame.GoBack();
            base.OnNavigatedTo(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartNow();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (! await ViewModel.Initialize())
                Frame.GoBack();
        }

        private void ResultWordListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListView;
            var item = listViewItem?.SelectedItem as Cell;
            if (item != null)
                ViewModel.LetterInResultWordTapped(item);
        }

        private void LetterArrayListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListView;
            var item = listViewItem?.SelectedItem as Cell;
            if (item != null)
                ViewModel.LetterInArrayTapped(item);
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanGoBack)
                Frame.GoBack();
            else
                ViewModel.Skip();
        }

        private async void Star_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;
            var img = sender as Image;
            if (img == null)
                return;
            var emptyStar = new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_empty.png"));
            var fullStar = new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_full.png"));
            switch (img.Name)
            {
                case "OneStarImage":
                    OneStarImage.Source = fullStar;
                    TwoStarImage.Source = ThreeStarImage.Source = FourStarImage.Source = FiveStarImage.Source = emptyStar;
                    ViewModel.PuzzleRated(1);
                    break;
                case "TwoStarImage":
                    OneStarImage.Source = TwoStarImage.Source = fullStar;
                    ThreeStarImage.Source = FourStarImage.Source = FiveStarImage.Source = emptyStar;
                    ViewModel.PuzzleRated(2);
                    break;
                case "ThreeStarImage":
                    OneStarImage.Source = TwoStarImage.Source = ThreeStarImage.Source = fullStar;
                    FourStarImage.Source = FiveStarImage.Source = emptyStar;
                    ViewModel.PuzzleRated(3);
                    break;
                case "FourStarImage":
                    OneStarImage.Source = TwoStarImage.Source = ThreeStarImage.Source = FourStarImage.Source = fullStar;
                    FiveStarImage.Source = emptyStar;
                    ViewModel.PuzzleRated(4);
                    break;
                case "FiveStarImage":
                    OneStarImage.Source = TwoStarImage.Source =
                        ThreeStarImage.Source = FourStarImage.Source = FiveStarImage.Source = fullStar;
                    ViewModel.PuzzleRated(5);
                    break;
            }
        }

        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            EndGameGrid.Margin = new Thickness(EndGameGrid.Margin.Left, EndGameGrid.Margin.Top,
                        EndGameGrid.Margin.Right, 20);
            ViewModel.Report();
        }

        private async void SendReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;
            var result = await ViewModel.SendReport();
            if (result == GameReportStatus.Ok)
                Frame.GoBack();
            else
            {
                var dialog = new MessageDialog("An error occurred when trying to send report. Please try again later.");
                await dialog.ShowAsync();
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelReport();
        }

        private async void ChallangeBtn_Click(object sender, RoutedEventArgs e)
        {
            EndGameGrid.Margin = new Thickness(EndGameGrid.Margin.Left, EndGameGrid.Margin.Top,
                        EndGameGrid.Margin.Right, 20);
            ProgRing.IsActive = true;
            await ViewModel.GoToChallenge();
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
            if (ViewModel.NeedDisplayEndGameControl && !ViewModel.NeedDisplayReportControl && !ViewModel.NeedDisplayCallengeControl)
            {
                //panelTrans.Y = -panelTrans.Y;
                //Storyboard anim = (Storyboard)this.Resources["mainInAnimation"];
                //anim.Begin();
                if (Math.Abs(EndGameGrid.Margin.Bottom - 20) < 0.01)
                    EndGameGrid.Margin = new Thickness(EndGameGrid.Margin.Left, EndGameGrid.Margin.Top,
                        EndGameGrid.Margin.Right, -180);
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
    }
}
