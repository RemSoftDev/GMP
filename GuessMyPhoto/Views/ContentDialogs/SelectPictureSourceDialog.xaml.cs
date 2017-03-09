using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using GuessMyPhoto.Enums;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views.ContentDialogs
{
    public sealed partial class SelectPictureSourceDialog : ContentDialog
    {
        public DialogResultEnum Result;
        public PictureSourceEnum SelectedPicrureStore;
        public SelectPictureSourceDialog(bool showFacebookBtn)
        {
            this.InitializeComponent();
            Result = DialogResultEnum.Cancel;
            if (showFacebookBtn)
                FacebookBtn.Visibility = Visibility.Visible;
        }

        private void FacebookBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultEnum.Accept;
            SelectedPicrureStore = PictureSourceEnum.Facebook;
            this.Hide();
        }

        private void CameraBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultEnum.Accept;
            SelectedPicrureStore = PictureSourceEnum.Camera;
            this.Hide();
        }

        private void PhotoGalleryBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultEnum.Accept;
            SelectedPicrureStore = PictureSourceEnum.Gallery;
            this.Hide();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = DialogResultEnum.Cancel;
            this.Hide();
        }
    }
}
