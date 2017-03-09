using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Enums;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GuessMyPhoto.Views.ContentDialogs
{
    public sealed partial class CroppImageContentDialog : ContentDialog
    {
        private double width;
        private double heigth;

        public DialogResultEnum DialogResult { get; set; }
        public WriteableBitmap CroppedImage { get; set; }
        public CroppImageContentDialog(WriteableBitmap wb, bool showCancelBtn = true)
        {
            this.InitializeComponent();
            CancelBtn.IsEnabled = showCancelBtn;
            DialogResult = DialogResultEnum.Cancel;
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            //var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            //var screenSize = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            this.ImageCropper.MaxHeight = bounds.Height - 150;
            if (wb.PixelWidth > 0)
                this.ImageCropper.Height = (this.ImageCropper.Width / wb.PixelWidth) * wb.PixelHeight;
            this.ImageCropper.SourceImage = wb;
            ProgRing.IsActive = true;
            this.ImageCropper.PropertyChanged += ImageCropper_PropertyChanged;
        }

        private void ImageCropper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
            ProgRing.IsActive = false;
            this.ImageCropper.PropertyChanged -= ImageCropper_PropertyChanged;
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            CroppedImage = this.ImageCropper.CroppedImage;
            DialogResult = DialogResultEnum.Accept;
            this.Hide();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResultEnum.Cancel;
            this.Hide();
        }
    }
}
