using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
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
    public sealed partial class AddNewGamePage : Page
    {
        private AddNewGameViewModel ViewModel;
        private StorageFile photo;
        private int angle;
        private double inputPaneHeight;
        public AddNewGamePage()
        {
            this.InitializeComponent();
            
            //Window.Current.CoreWindow.KeyDown += KeyDownEventHandler;
            //Window.Current.CoreWindow.KeyUp += KeyEventHandlerDevNull;
        }

        
        //private void KeyEventHandlerDevNull(CoreWindow sender, KeyEventArgs args)
        //{
        //    args.Handled = true;
        //}

        //private void KeyDownEventHandler(CoreWindow sender, KeyEventArgs args)
        //{
        //    if (args.VirtualKey >= VirtualKey.A && args.VirtualKey <= VirtualKey.Z)
        //    {
                
        //    }
        //    args.Handled = true;
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ViewModel = e.Parameter as AddNewGameViewModel;
            }
            if (ViewModel == null)
                Frame.GoBack();
            base.OnNavigatedTo(e);
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
            InputPane.GetForCurrentView().Showing += Pane_Showing;
            InputPane.GetForCurrentView().Hiding += Pane_Hiding;
            if (ViewModel.PictureSource == PictureSourceEnum.Camera)
                await MakeNewPhoto();
            else
                OpenButton_Click(null, null);
        }

        private void Pane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            EnterWordGrid.Margin = new Thickness(EnterWordGrid.Margin.Left, EnterWordGrid.Margin.Top, EnterWordGrid.Margin.Right, inputPaneHeight);
        }

        private void Pane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            EnterWordGrid.Margin = new Thickness(EnterWordGrid.Margin.Left, EnterWordGrid.Margin.Top, EnterWordGrid.Margin.Right, args.OccludedRect.Height + 15);
        }

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            args.Handled = true;
            ViewModel.KeyPressed(Convert.ToChar(args.KeyCode));
        }

        private async Task MakeNewPhoto()
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            
            photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            UpdatePhoto();
        }
        private async Task<WriteableBitmap> Rotate()
        {
            WriteableBitmap bitmap;
            using (IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, stream);
                uint width = decoder.PixelWidth;
                uint height = decoder.PixelHeight;
                if (angle % 180 != 0)
                {
                    width = decoder.PixelHeight;
                    height = decoder.PixelWidth;
                }
                Dictionary<int, BitmapRotation> angles = new Dictionary<int, BitmapRotation>()
            {
                { 0, BitmapRotation.None },
                { 90,  BitmapRotation.Clockwise90Degrees },
                { 180,  BitmapRotation.Clockwise180Degrees },
                { 270, BitmapRotation.Clockwise270Degrees },
                { 360, BitmapRotation.None }
            };
                BitmapTransform transform = new BitmapTransform();
                transform.Rotation = angles[angle];
                PixelDataProvider data = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, transform,
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                bitmap = new WriteableBitmap((int)width, (int)height);
                byte[] buffer = data.DetachPixelData();
                using (Stream pixels = bitmap.PixelBuffer.AsStream())
                {
                    pixels.Write(buffer, 0, (int)pixels.Length);
                }
            }
            return bitmap;
        }
        private async void CameraBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (photo != null)
            //    await photo.DeleteAsync();
            await MakeNewPhoto();
        }

        private async void RotateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (angle == 360) angle = 0;
            angle += 90;
            var result = await Rotate();
            ViewModel.Photo = result;
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
            InputPane.GetForCurrentView().Showing -= Pane_Showing;
            InputPane.GetForCurrentView().Hiding -= Pane_Hiding;
            if (photo != null)
                await photo.DeleteAsync();
        }

        private async void CropButton_Click(object sender, RoutedEventArgs e)
        {
            await CropPhoto(true);
        }

        private async Task CropPhoto(bool showCancelBtn)
        {
            WriteableBitmap wb = ViewModel.Photo;
            CroppImageContentDialog dialog = new CroppImageContentDialog(wb, showCancelBtn);
            if (!showCancelBtn)
                dialog.Closing += Dialog_Closing;
            await dialog.ShowAsync();
            if (!showCancelBtn)
                dialog.Closing -= Dialog_Closing;
            if (dialog.DialogResult == DialogResultEnum.Accept && dialog.CroppedImage != null)
            {
                ViewModel.Photo = dialog.CroppedImage;
            }
        }

        private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            var dialog = sender as CroppImageContentDialog;
            if (dialog?.DialogResult == DialogResultEnum.Cancel)
            {
                args.Cancel = true;
                // Handle back press here instead of closing.
            }
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtomAppBar.Visibility = Visibility.Collapsed;
            GridVithPhoto.Visibility = Visibility.Collapsed;
            ViewModel.GoToLanguageSelection();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.IsEnabled = false;
            ViewModel.GoToScrambling();
        }

        private void BackToLanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.IsEnabled = false;
            ViewModel.GoToLanguageSelection();
        }

        private void ListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (InputTextBox.FocusState == FocusState.Unfocused)
                InputTextBox.Focus(FocusState.Programmatic);
        }

        private void EnterWordGrid_Loaded(object sender, RoutedEventArgs e)
        {
            inputPaneHeight = EnterWordGrid.Margin.Bottom;
        }

        private void BackToEnterBtn_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.IsEnabled = true;
            InputTextBox.Focus(FocusState.Programmatic);
            ViewModel.GoToEnterWord(ViewModel.PuzzleData.Language);
        }

        private void Scramble_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Scramble();
        }

        private async void PostPuzzle_Click(object sender, RoutedEventArgs e)
        {
            if (!await LogicHelper.IsInternet())
                return;
            ProgRing.IsActive = true;
            BackToEnterBtn.IsEnabled = false;
            Scramble.IsEnabled = false;
            PostPuzzle.IsEnabled = false;
            var result = await ViewModel.PostPuzzle();
            ProgRing.IsActive = false;
            BackToEnterBtn.IsEnabled = true;
            Scramble.IsEnabled = true;
            PostPuzzle.IsEnabled = true;

            var dialog = new MessageDialog("", "");

            if (result?.Status != GameCreateStatus.Ok)
            {
                dialog.Title = "Failed to create puzzle";
                dialog.Content = "An error occurred when trying to create puzzle. Please try again later.";
                await dialog.ShowAsync();
            }
            
            //if (result?.Status == GameCreateStatus.Ok)
            //{
            //    Frame.GoBack();
            //}
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            //ProgRing.IsActive = true;
            //if (photo != null)
            //    await photo.DeleteAsync();
            StorageFile imgFile = await LogicHelper.OpenImage();
            photo = imgFile;
            UpdatePhoto();
            //ProgRing.IsActive = false;
        }

        private async void UpdatePhoto()
        {
            if (photo != null)
            {
                var wb = new WriteableBitmap(1, 1);
                await wb.LoadAsync(photo);
                ViewModel.Photo = wb;
                ContinueBtn.IsEnabled = true;
                RotateButton.IsEnabled = true;
                CropButton.IsEnabled = true;
                await CropPhoto(false);
            }
        }

        private void BackToPhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GoToPhotoSelection();
            ButtomAppBar.Visibility = Visibility.Visible;
            GridVithPhoto.Visibility = Visibility.Visible;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var control = sender as Image;
            if (control != null)
            {
                LanguageEnum lang;
                switch (control.Name)
                {
                    case "Image_es":
                        lang = LanguageEnum.Spanish;
                        break;
                    case "Image_dk":
                        lang = LanguageEnum.Danish;
                        break;
                    case "Image_se":
                        lang = LanguageEnum.Swedish;
                        break;
                    case "Image_no":
                        lang = LanguageEnum.Norwegian;
                        break;
                    default:
                        lang = LanguageEnum.English;
                        break;
                }
                InputTextBox.IsEnabled = true;
                InputTextBox.Focus(FocusState.Programmatic);
                ViewModel.GoToEnterWord(lang);
            }
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void ShareBtn_Click(object sender, RoutedEventArgs e)
        {
            ProgRing.IsActive = true;
            ShareBtn.IsEnabled = false;
            DoneBtn.IsEnabled = false;
            FacebookHelper fb = new FacebookHelper();
            await fb.ShareOnFb();
            ProgRing.IsActive = false;
            ShareBtn.IsEnabled = true;
            DoneBtn.IsEnabled = true;
            //Frame.GoBack();
        }

        private void Photo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Photo1.ActualHeight > Photo1.ActualWidth)
                Photo1.Height = Photo1.ActualWidth;
            else
                Photo1.Width = Photo1.ActualHeight;
        }
    }
}
