using GuessMyPhoto.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;

namespace GuessMyPhoto.Models.Game
{

    public class GameModel
    {
        [JsonProperty(PropertyName = "gid")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string CreatorUid { get; set; }
        
        [JsonProperty(PropertyName = "uname")]
        public string CreatorName { get; set; }
        public int GameType { get; set; }
        public string CreateGameType { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "word")]
        public string Word { get; set; }


        [JsonProperty(PropertyName = "owls")]
        public int Owls { get; set; }

        [JsonProperty(PropertyName = "stars")]
        public int Stars { get; set; }

        [JsonProperty(PropertyName = "total-score")]
        public int TotalScore { get; set; }

        [JsonProperty(PropertyName = "total-played")]
        public int TotalPlayed { get; set; }

        //[JsonProperty(PropertyName = "brand-url")]
        //public string BrandUrl { get; set; }

        //[JsonProperty(PropertyName = "brand-ads")]
        //public string BrandAds { get; set; }

        public WriteableBitmap Photo { get; set; }
    }

    public class PlayerModel : INotifyPropertyChanged
    {
        private string userId;
        [JsonProperty(PropertyName = "pnum")]
        public int Position { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
                if (!string.IsNullOrWhiteSpace(userId))
                    LogicHelper.DownloadPhoto(LogicHelper.CreateImageUrl(ImageTypeToDownload.ProfilePhotoSmall, userId), UpdateProfileImage);
            }
        }

        [JsonProperty(PropertyName = "uname")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

        public string DisplayName => $"{Position}. {UserName}";
        public ImageSource UserPhoto { get; set; }
        
        private void UpdateProfileImage(BitmapImage bitmap)
        {
            UserPhoto = bitmap;
            OnPropertyChanged(nameof(UserPhoto));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameCreateResponseModel
    {
        public GameCreateStatus Status { get; set; }
        public string GameId { get; set; }
    }

    public class ChallengeCreateResponseModel
    {
        public CreateChallengeStatus Status { get; set; }
        public string ChallengeId { get; set; }
    }

    public class GameStartResponse
    {
        public GameStartStatus Status
        {
            get
            {
                switch (StatusCode)
                {
                    case 100:
                        return GameStartStatus.Ok;
                    case 101:
                        return GameStartStatus.HashError;
                    case 102:
                        return GameStartStatus.AlredyPlayedOrStarted;
                    case 103:
                        return GameStartStatus.DbError;
                    default:
                        return GameStartStatus.OtherIssues;
                }
            }
            
        }

        [JsonProperty(PropertyName = "status")]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "word_chars")]
        public int WordLength { get; set; }

        [JsonProperty(PropertyName = "word")]
        public string WordToGuess { get; set; }

        [JsonProperty(PropertyName = "volapyk")]
        public string Leters { get; set; }
    }

    public class GameEndResponse
    {
        public GameEndStatus Status
        {
            get
            {
                switch (StatusCode)
                {
                    case 100:
                        return GameEndStatus.Ok;
                    case 101:
                        return GameEndStatus.HashError;
                    case 102:
                        return GameEndStatus.NotPlayedOrNotStarted;
                    case 103:
                        return GameEndStatus.DbError;
                    default:
                        return GameEndStatus.HashError;
                }
            }
        }

        [JsonProperty(PropertyName = "status")]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public string GameId { get; set; }
    }
}
