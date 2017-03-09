using GuessMyPhoto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.ViewModels;

namespace GuessMyPhoto.Models.User
{
    public class UserNativeLoginResponse
    {
        public NativeLoginStatus Status { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string FacebookId { get; set; }
    }

    public class UserFbLoginResponse
    {
        public FbLoginStatus Status { get; set; }
        public string UserId { get; set; }
        public string FbId { get; set; }
    }

    public class UserModel
    {
        private string rating;
        //string email, string name, string password, string countryCode, 
        //string phoneNumber = null, string fbId = null
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FbId { get; set; }
        public LanguageEnum SelectedLanguage
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                var item = localSettings.Values["UserLanguage"];
                if (item == null)
                {
                    return LanguageEnum.English;
                }
                return (LanguageEnum) localSettings.Values["UserLanguage"];
            }
            set
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["UserLanguage"] = (int)value;
            }
        }
        public int TotalPlayedPuzzles { get; set; }
        public int TotalCreatedPuzzles { get; set; }

        public string TotalRating
        {
            get
            {
                if (string.IsNullOrWhiteSpace(rating))
                    return "0-000";
                return rating;
            }
            set { rating = value; }
        }

        public int TotalScore { get; set; }
        public WriteableBitmap ProfilePhoto { get; set; }

        public UserModel GetCopy()
        {
            return this.MemberwiseClone() as UserModel;
        }
    }

    public class UserDataUpdateResponse
    {
        public UserDataUpdateStatus Status { get; set; }
        public string Response { get; set; }
    }

    public class UserCreateResponse
    {
        public RegisterStatus Status { get; set; }
        public string UserId { get; set; }
    }

    public class UserDisconnectFbResponse
    {
        public FbDisconnectStatus Status { get; set; }
        public string Response { get; set; }
    }

    public class UserForgotPasswordResponse
    {
        public UserForgotPasswordStatus Status { get; set; }
        public string Response { get; set; }
    }
}
