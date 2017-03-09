using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Models;
using GuessMyPhoto.Enums;
using GuessMyPhoto.ViewModels;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using LanguageEnum = GuessMyPhoto.Enums.LanguageEnum;

namespace GuessMyPhoto
{
    public static class LogicHelper
    {
        private static string apiUrl = App.ApiUrl;

        public static async Task<StorageFile> OpenImage()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile imgFile = await openPicker.PickSingleFileAsync();

            return imgFile;
        }

        public static async Task<BitmapImage> GetBluredImage(string url)
        {
            BitmapImage image = new BitmapImage();
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget offscreen = new CanvasRenderTarget(device, 400, 150, 96);
            try
            {
                var cbi = await CanvasBitmap.LoadAsync(device, new Uri(url));

                using (var ds = offscreen.CreateDrawingSession())
                {
                    var blur = new GaussianBlurEffect();
                    blur.BlurAmount = 15f;
                    blur.Source = cbi;
                    ds.DrawImage(blur);
                }

                using (var stream = new InMemoryRandomAccessStream())
                {
                    stream.Seek(0);
                    await offscreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);


                    image.SetSource(stream);
                }
            }
            catch
            {
            }
            return image;
        }
        public static async Task<string> GetPictureBytes(StorageFile image)
        {
            if (image == null)
            {
                return null;
            }
            string myString;
            Stream stream = (await image.OpenReadAsync()).AsStreamForRead();
            using (BinaryReader br = new BinaryReader(stream))
            {
                byte[] bin = br.ReadBytes(Convert.ToInt32(stream.Length));
                myString = Convert.ToBase64String(bin);
            }

            //using (var stream = await image.OpenReadAsync())
            //{
            //    res = new byte[stream.Size];
            //    using (var reader = new DataReader(stream))
            //    {
            //        await reader.LoadAsync((uint)stream.Size);
            //        reader.ReadBytes(res);
            //    }
            //}

            return myString;
        }
        public async static Task<bool> IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            if (!internet)
            {
                var dialog = new MessageDialog("The app need accsess to the internet!", "No internet access!");
                await dialog.ShowAsync();
            }
            return internet;
        }
        public static string CreateUrl(string requestUrl, List<string> prms)
        {
            string res = requestUrl;
            foreach (string param in prms)
            {
                res += param;
            }

            return res;
        }
        public static string CreateMD5(string param, string salt)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(
                param + salt, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }

        public static async Task<List<CountryCode>> GetCountriesCodes()
        {
            List<string> stringsList = new List<string>();
            var contriesCodes = new List<CountryCode>();
            //var file = await Package.Current.InstalledLocation.TryGetItemAsync("CountriesCodes.csv") as StorageFile;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///DataFiles/CountriesCodes.txt"));
            using (var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                while (streamReader.Peek() >= 0)
                {
                    stringsList.Add(streamReader.ReadLine());
                }
            }
            char[] delimiterChars = {','};
            string[] words;
            foreach (var str in stringsList)
            {
                words = str.Split(delimiterChars, StringSplitOptions.None);
                if(words.Length >= 3 && !String.IsNullOrWhiteSpace(words[0]) && !String.IsNullOrWhiteSpace(words[1]))
                    contriesCodes.Add(new CountryCode {ContryName = words[0], PhoneCode = words[1], ContryCode = words[2]});
            }
            return contriesCodes;
        }

        public static string GetUserIdFromStorage()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            return localSettings.Values["UserId"] as string;
        }

        public static void SetUserIdToStorage(string id)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["UserId"] = id;
        }

        public static string GetLangString(LanguageEnum language)
        {
            switch (language)
            {
                case LanguageEnum.Danish:
                    return "dk";
                case LanguageEnum.Spanish:
                    return "es";
                case LanguageEnum.Norwegian:
                    return "no";
                case LanguageEnum.Swedish:
                    return "se";
                default:
                    return "uk";
            }
        }

        public delegate void CallBackDelegate(BitmapImage image);
        public static async void DownloadPhoto(string urlString, CallBackDelegate updateProfileImage)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(new Uri(urlString));

                    BitmapImage bitmap = new BitmapImage();

                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var memStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memStream);
                                memStream.Position = 0;

                                bitmap.SetSource(memStream.AsRandomAccessStream());
                            }
                        }
                        if (bitmap.PixelHeight > 10 && bitmap.PixelWidth > 10)
                        {
                            updateProfileImage(bitmap);
                        }

                    }
                }
                catch (Exception)
                {
                    
                }
            }
        }

        public static string CreateImageUrl(ImageTypeToDownload itype, string param)
        {
            string requestUrl = "";
            switch (itype)
            {
                case ImageTypeToDownload.GamePhotoMaxSplit:
                case ImageTypeToDownload.GamePhotoMidSplit:
                case ImageTypeToDownload.GamePhotoNoSplit:
                    requestUrl = "media_9a8fee7abcdd38b5f909d5111ae67fab/";
                    break;
                case ImageTypeToDownload.ProfilePhotoBig:
                case ImageTypeToDownload.ProfilePhotoMedium:
                case ImageTypeToDownload.ProfilePhotoSmall:
                    requestUrl = "media_profile_f199d358b1d0ccef39dad36b1d8b9364/";
                    break;
            }
            string fullUrl = apiUrl + requestUrl + param;
            switch (itype)
            {
                case ImageTypeToDownload.ProfilePhotoSmall:
                case ImageTypeToDownload.GamePhotoNoSplit:
                    fullUrl += "_A.jpg";
                    break;
                case ImageTypeToDownload.ProfilePhotoMedium:
                case ImageTypeToDownload.GamePhotoMidSplit:
                    fullUrl += "_B.jpg";
                    break;
                case ImageTypeToDownload.GamePhotoMaxSplit:
                case ImageTypeToDownload.ProfilePhotoBig:
                    fullUrl += "_C.jpg";
                    break;
            }

            return fullUrl;
        }

        public static async Task<WriteableBitmap> ResizeWritableBitmap(WriteableBitmap baseWriteBitmap, uint width, uint height)
        {
            // Get the pixel buffer of the writable bitmap in bytes
            Stream stream = baseWriteBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[(uint)stream.Length];
            await stream.ReadAsync(pixels, 0, pixels.Length);
            //Encoding the data of the PixelBuffer we have from the writable bitmap
            var inMemoryRandomStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream);
            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, width, height, 96, 96, pixels);
            await encoder.FlushAsync();
            // At this point we have an encoded image in inMemoryRandomStream
            // We apply the transform and decode
            var transform = new BitmapTransform
            {
                ScaledWidth = width,
                ScaledHeight = height
            };
            inMemoryRandomStream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(inMemoryRandomStream);
            var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Rgba8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage);
            //An array containing the decoded image data
            var sourceDecodedPixels = pixelData.DetachPixelData();
            // Approach 1 : Encoding the image buffer again:
            //Encoding data
            var inMemoryRandomStream2 = new InMemoryRandomAccessStream();
            var encoder2 = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream2);
            encoder2.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, width, height, 96, 96, sourceDecodedPixels);
            await encoder2.FlushAsync();
            inMemoryRandomStream2.Seek(0);
            // finally the resized writablebitmap
            var bitmap = new WriteableBitmap((int)width, (int)height);
            await bitmap.SetSourceAsync(inMemoryRandomStream2);
            return bitmap;
        }
    }
}
