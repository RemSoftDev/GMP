using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models;
using GuessMyPhoto.Models.User;

namespace GuessMyPhoto.ViewModels
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private CountryCode selectedCountry;
        private List<CountryCode> countryCodes;
        private UserModel copyUser;
        private UserModel user;
        private bool connectedToFb = false;
        private bool newPhotoSelected = false;
        private WriteableBitmap profilePhoto;

        public UserModel UserData
        {
            get { return user; }
            set { user = value; }
        }

        public string FacebookConnStat
        {
            get
            {
                if (connectedToFb)
                    return "Connected to FB";
                return "Disconnected from FB";
            }
        }
        public ImageSource Image { get; set; }
        public List<string> CountriesCodes => countryCodes.Select(r=>r.CountryWithCode).ToList();
        public int SelectedCountryIndex
        {
            get
            {
                return countryCodes.IndexOf(selectedCountry);
            }
            set
            {
                if (value < 0 || value > countryCodes.Count - 1)
                {
                    selectedCountry = countryCodes.FirstOrDefault();
                }
                else
                {
                    selectedCountry = countryCodes[value];
                }
                user.CountryCode = selectedCountry.PhoneCodeDigitOnly;
            }
        }
        public UserViewModel(UserModel user = null)
        {
            if (user == null)
                this.user = new UserModel();
            else
            {
                this.user = user;
                copyUser = this.user.GetCopy();
                connectedToFb = AppInfo.AppUser?.FbId != null && AppInfo.AppUser?.FbId != "no_email_accept" && AppInfo.AppUser?.FbId != "no_facebook";
                UserDataUpdated();
            }

            this.Image = new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/btn_owl.png"));
            LogicHelper.DownloadPhoto(LogicHelper.CreateImageUrl(ImageTypeToDownload.ProfilePhotoBig, this.user.UserId), PhotoDownloaded);

            countryCodes = Task.Run(async () => await LogicHelper.GetCountriesCodes()).Result;
            
            if (string.IsNullOrWhiteSpace(this.user.CountryCode))
            {
                var region = new GeographicRegion().CodeThreeLetter;
                var item = countryCodes.FirstOrDefault(r => r.ContryCode == region);
                if (item != null)
                    selectedCountry = item;
                else
                    selectedCountry = countryCodes.FirstOrDefault();
                this.user.CountryCode = selectedCountry.PhoneCodeDigitOnly;
            }
            else
            {
                var item = countryCodes.FirstOrDefault(r => r.PhoneCodeDigitOnly == this.user.CountryCode);
                if (item != null)
                    selectedCountry = item;
                else
                    selectedCountry = countryCodes.FirstOrDefault();
            }
        }

        private async void UserDataUpdated()
        {
            await User.GetUserData(this.user.UserId);
            user = AppInfo.AppUser;
            OnPropertyChanged(nameof(UserData));
        }
        public void UpdateImage(WriteableBitmap image, bool newPhotoSelected = false)
        {
            this.newPhotoSelected = newPhotoSelected;
            profilePhoto = image;
            this.Image = profilePhoto;
            user.ProfilePhoto = image;
            OnPropertyChanged(nameof(Image));
        }

        private void PhotoDownloaded(BitmapImage bitmap)
        {
            this.Image = bitmap;
            OnPropertyChanged(nameof(Image));
        }
        public async Task<UserDataUpdateResponse> SaveUserData()
        {
            UploadUserPhoto();
            return await User.DataUpdate(user);
        }
        public async Task<UserCreateResponse> CreateNewUser()
        {
            var result = await User.CreateUser(user);
            if (result.Status == RegisterStatus.Ok)
            {
                copyUser = null;
            }
            //else
            //{
            //    var dialog = new MessageDialog("Can't connect to server. Check your internet connection", "Connection error");
            //    await dialog.ShowAsync();
            //}
            return result;
        }

        private void UploadUserPhoto()
        {
            if (profilePhoto != null && newPhotoSelected)
                User.UploadProfilePhoto(user.UserId, profilePhoto);
        }
        public void Logout()
        {
            LogicHelper.SetUserIdToStorage(string.Empty);
            AppInfo.AppUser.SelectedLanguage = LanguageEnum.English;
            AppInfo.DeinitializeUser();
        }
        public async Task ConnectOrDisconnectFromFb()
        {
            if (!connectedToFb)
            {
                UserFbLoginResponse loginResult = await User.LoginFb();
                if (loginResult?.Status == FbLoginStatus.Ok)
                {
                    connectedToFb = AppInfo.AppUser?.FbId != null && AppInfo.AppUser.FbId != "no_email_accept" && AppInfo.AppUser.FbId != "no_facebook";
                    OnPropertyChanged(nameof(FacebookConnStat));
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Failed connect to Facebook!", "Can't connect");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                var response = await User.DisconnectFb(AppInfo.AppUser);
                if (response.Status == FbDisconnectStatus.Ok)
                {
                    connectedToFb = AppInfo.AppUser?.FbId != null && AppInfo.AppUser.FbId != "no_email_accept" && AppInfo.AppUser.FbId != "no_facebook";
                    OnPropertyChanged(nameof(FacebookConnStat));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
