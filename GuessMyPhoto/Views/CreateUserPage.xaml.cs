using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using CroppControl.Helpers;
using GuessMyPhoto.Enums;
using GuessMyPhoto.ViewModels;
using GuessMyPhoto.Views.ContentDialogs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateUserPage : Page
    {
        private TextBox notValidTextBox = null;
        public UserViewModel ViewModel;
        public CreateUserPage()
        {
            ViewModel = new UserViewModel();
            this.InitializeComponent();
        }
        private async void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            var pictureDialog = new SelectPictureSourceDialog(false);
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
            }
            else
            {
                CameraCaptureUI captureUI = new CameraCaptureUI();
                captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
                imgFile = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            }
            await UpdatePhoto(imgFile);
            ProgRing.IsActive = false;
        }
        private async Task UpdatePhoto(StorageFile file)
        {
            if (file != null)
            {
                var wb = new WriteableBitmap(1, 1);
                await wb.LoadAsync(file);
                CroppImageContentDialog dialog = new CroppImageContentDialog(wb);
                await dialog.ShowAsync();
                if (dialog.DialogResult == DialogResultEnum.Accept && dialog.CroppedImage != null)
                {
                    ViewModel.UpdateImage(dialog.CroppedImage, true);
                }
            }
        }
        private async void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!AllInputFieldsIsValid())
                return;
            if (!await LogicHelper.IsInternet())
                return;
            CreateBtn.IsEnabled = false;
            ChangePictureBtn.IsEnabled = false;
            ProgRing.IsActive = true;
            
            var result = await ViewModel.CreateNewUser();
            if (result != null)
            {
                ProgRing.IsActive = false;
                CreateBtn.IsEnabled = true;
                ChangePictureBtn.IsEnabled = true;

                if (result.Status == RegisterStatus.Ok)
                    Frame.Navigate(typeof(MainPage));
                else
                {
                    var dialog = new MessageDialog("", "Failed to create user");
                    switch (result.Status)
                    {
                        case RegisterStatus.EmailError:
                            dialog.Content = "The Email is not valid!";
                            break;
                        case RegisterStatus.PasswordError:
                            dialog.Content = "Password contains invalid characters. Enter the another password.";
                            break;
                        case RegisterStatus.UserExistsOnEmail:
                            dialog.Content = $"User with email '{ViewModel.UserData.Email}' already exist";
                            break;
                        default:
                            dialog.Content = "An error occurred when trying to create a user. Please try again later.";
                            break;
                    }
                    await dialog.ShowAsync();
                }
            }
            else
            {
                var dialog = new MessageDialog("Internal server error. Please try again later.", "Failed to create user");
                await dialog.ShowAsync();
            }
        }

        private bool AllInputFieldsIsValid()
        {
            notValidTextBox = null;
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.Name))
            {
                FlyoutTextBlock.Text = "You must enter your name!";
                comboBoxEmptyFlyout.ShowAt(NameTextBox);
                notValidTextBox = NameTextBox;
                return false;
            }
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.Email))
            {
                FlyoutTextBlock.Text = "You must enter your Email!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                notValidTextBox = EmailTextBox;
                return false;
            }
            if (!Regex.IsMatch(ViewModel.UserData.Email, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                FlyoutTextBlock.Text = "The email is invalid!";
                comboBoxEmptyFlyout.ShowAt(EmailTextBox);
                notValidTextBox = EmailTextBox;
                return false;
            }
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.Password))
            {
                FlyoutTextBlock.Text = "You must enter a password!";
                comboBoxEmptyFlyout.ShowAt(PassTextBox);
                return false;
            }
            if (ViewModel.UserData.Password != PassConfTextBox.Password)
            {
                FlyoutTextBlock.Text = "Your password and confirmation password do not match!";
                comboBoxEmptyFlyout.ShowAt(PassTextBox);
                return false;
            }
            if (string.IsNullOrWhiteSpace(ViewModel.UserData.PhoneNumber))
            {
                FlyoutTextBlock.Text = "You must enter your phone number!";
                comboBoxEmptyFlyout.ShowAt(PhoneTextBox);
                notValidTextBox = PhoneTextBox;
                return false;
            }
            if (!Regex.IsMatch(ViewModel.UserData.PhoneNumber, @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$"))
            {
                FlyoutTextBlock.Text = "You must enter your phone number!";
                comboBoxEmptyFlyout.ShowAt(PhoneTextBox);
                notValidTextBox = PhoneTextBox;
                return false;
            }
            return true;
        }

        private void comboBoxEmptyFlyout_Closed(object sender, object e)
        {
            notValidTextBox?.Focus(FocusState.Programmatic);
        }
    }
}
