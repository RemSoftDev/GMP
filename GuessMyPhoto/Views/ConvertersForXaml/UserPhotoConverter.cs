using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace GuessMyPhoto.Views.ConvertersForXaml
{
    public class UserPhotoConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var image = value as ImageSource;
            if (image != null)
            {
                return image;
            }
            return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/profile_picture.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
