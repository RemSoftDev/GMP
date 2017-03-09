using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using CroppControl.Helpers;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.User;
using GuessMyPhoto.ViewModels;
using GuessMyPhoto.Views.ContentDialogs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyProfilePage : Page
    {
        private TextBox notValidTextBox = null;
        public UserViewModel ViewModel;
        public MyProfilePage()
        {
            ViewModel = new UserViewModel(AppInfo.AppUser);
            this.InitializeComponent();
            //var pane = InputPane.GetForCurrentView();
            //pane.Showing += Pane_Showing;
            //pane.Hiding += Pane_Hiding;
        }

        //private void Pane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        //{
        //    this.ScrollViewer.Height = this.ActualHeight;
        //}

        //private void Pane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        //{
        //    this.ScrollViewer.Height = this.ActualHeight - args.OccludedRect.Height - 50;
        //}

        private async void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            var pictureDialog = new SelectPictureSourceDialog(AppInfo.AppUser?.FbId != null && AppInfo.AppUser.FbId != "no_email_accept" && AppInfo.AppUser.FbId != "no_facebook");
            await pictureDialog.ShowAsync();
            if (pictureDialog.Result == DialogResultEnum.Cancel)
            {
                return;
            }

            ProgRing.IsActive = true;
            StorageFile imgFile;
            if (pictureDialog.SelectedPicrureStore == PictureSourceEnum.Gallery)
            {
                imgFile = await LogicHelper.OpenImage();
                await UpdatePhoto(imgFile);
            }
            else if(pictureDialog.SelectedPicrureStore == PictureSourceEnum.Camera)
            {
                CameraCaptureUI captureUI = new CameraCaptureUI();
                captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
                imgFile = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
                await UpdatePhoto(imgFile);
            }
            else
            {
                FacebookHelper fb = new FacebookHelper();
                string pictureUrl = await fb.GetFbProfilePicture(AppInfo.AppUser.FbId);
                if (!string.IsNullOrWhiteSpace(pictureUrl))
                {
                    WriteableBitmap wbm = new WriteableBitmap(320, 320);
                    var httpClient = new HttpClient();
                    var buffer = await httpClient.GetBufferAsync(new Uri(pictureUrl));

                    using (var stream = buffer.AsStream())
                    {
                        await wbm.SetSourceAsync(stream.AsRandomAccessStream());
                    }
                    //ViewModel.UpdateImage(wbm, true);
                    await CropImage(wbm);

                }
            }
            
            ProgRing.IsActive = false;
        }
        private async Task UpdatePhoto(StorageFile file)
        {
            if (file != null)
            {
                var wb = new WriteableBitmap(1, 1);
                await wb.LoadAsync(file);
                await CropImage(wb);
            }
        }

        private async Task CropImage(WriteableBitmap wb)
        {
            if (wb.PixelHeight <= 60 || wb.PixelWidth <= 60)
            {
                ViewModel.UpdateImage(wb, true);
                return;
            }
            CroppImageContentDialog dialog = new CroppImageContentDialog(wb);
            await dialog.ShowAsync();
            if (dialog.DialogResult == DialogResultEnum.Accept && dialog.CroppedImage != null)
            {
                ViewModel.UpdateImage(dialog.CroppedImage, true);
            }
        }

        private async void ConnectToFbBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;
            ProgRing.IsActive = true;
            await ViewModel.ConnectOrDisconnectFromFb();
            ProgRing.IsActive = false;
        }

        private void comboBoxEmptyFlyout_Closed(object sender, object e)
        {
            notValidTextBox?.Focus(FocusState.Programmatic);
        }

        private async void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            ViewModel.Logout();
            ProgRing.IsActive = false;
            Frame.Navigate(typeof(StartPage));
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            notValidTextBox = null;
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.Name))
            {
                FlyoutTextBlock.Text = "You must enter your name!";
                comboBoxEmptyFlyout.ShowAt(NameTextBox);
                notValidTextBox = NameTextBox;
                return;
            }
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.Email))
            {
                FlyoutTextBlock.Text = "You must enter your Email!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                notValidTextBox = EmailTextBox;
                return;
            }
            if (!Regex.IsMatch(ViewModel.UserData.Email, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                FlyoutTextBlock.Text = "The email is invalid!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                notValidTextBox = EmailTextBox;
                return;
            }
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.PhoneNumber))
            {
                FlyoutTextBlock.Text = "You must enter your phone number!";
                comboBoxEmptyFlyout.ShowAt(PhoneTextBox);
                notValidTextBox = PhoneTextBox;
                return;
            }
            if (!Regex.IsMatch(ViewModel.UserData.PhoneNumber, @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$"))
            {
                FlyoutTextBlock.Text = "You must enter your phone number!";
                comboBoxEmptyFlyout.ShowAt(PhoneTextBox);
                notValidTextBox = PhoneTextBox;
                return;
            }

            if (!await LogicHelper.IsInternet())
                return;

            LogoutBtn.IsEnabled = false;
            ChangePictureBtn.IsEnabled = false;
            ConnectToFbBtn.IsEnabled = false;
            SaveBtn.IsEnabled = false;
            ProgRing.IsActive = true;
            var result = await ViewModel.SaveUserData();
            ProgRing.IsActive = false;
            LogoutBtn.IsEnabled = true;
            ChangePictureBtn.IsEnabled = true;
            ConnectToFbBtn.IsEnabled = true;
            SaveBtn.IsEnabled = true;
            var dialog = new MessageDialog("","");
            if (result?.Status == UserDataUpdateStatus.Ok)
            {
                //Frame.Navigate(typeof(MainPage));
                dialog.Content = "User data updated successfully!";
            }
            else
            {
                //Frame.Navigate(typeof(MainPage));
                dialog.Title = "Can't update user data";
                dialog.Content = "An error occurred when trying to update user data. Please try again later.";
            }
            await dialog.ShowAsync();
        }
    }
}
