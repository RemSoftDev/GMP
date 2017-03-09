using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.ViewModels;
using GuessMyPhoto.Enums;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace GuessMyPhoto.Models.Game
{
    public class Puzzle :INotifyPropertyChanged
    {
        public Puzzle()
        {
        }

        public Puzzle(GameModel model)
        {
            CreatorName = model.CreatorName;
            Rating = model.Stars;
            Type = model.GameType;
            OWLS = model.Owls;
            GameId = model.GameId;
            Word = model.Word;
            switch (model.Language)
            {
                case "us":
                    Language = LanguageEnum.English;
                    break;
                case "uk":
                    Language = LanguageEnum.English;
                    break;
                case "es":
                    Language = LanguageEnum.Spanish;
                    break;
                case "se":
                    Language = LanguageEnum.Swedish;
                    break;
                case "dk":
                    Language = LanguageEnum.Danish;
                    break;
                case "no":
                    Language = LanguageEnum.Norwegian;
                    break;
            }
            creatorPhotoUrl = LogicHelper.CreateImageUrl(ImageTypeToDownload.ProfilePhotoSmall, model.CreatorUid);
            gamePhotoMaxSplitUrl = LogicHelper.CreateImageUrl(ImageTypeToDownload.GamePhotoMaxSplit, model.GameId);
            gamePhotoMidSplitUrl = LogicHelper.CreateImageUrl(ImageTypeToDownload.GamePhotoMidSplit, model.GameId);
            gamePhotoNoSplitUrl = LogicHelper.CreateImageUrl(ImageTypeToDownload.GamePhotoNoSplit, model.GameId);
        }

        public string GameId { get; set; }
        public string CreatorName { get; set; }
        public ImageSource CreatorPhoto { get; set; }
        public LanguageEnum Language { get; set; }
        public int Rating { get; set; }
        public int Type { get; set; }
        public int OWLS { get; set; }
        public string Word { get; set; }
        public string GamePhotoMaxSplitUrl => gamePhotoMaxSplitUrl;
        public string GamePhotoMidSplitUrl => gamePhotoMidSplitUrl;
        public string GamePhotoNoSplitUrl => gamePhotoNoSplitUrl;

        private readonly string creatorPhotoUrl;
        private readonly string gamePhotoMaxSplitUrl;
        private readonly string gamePhotoMidSplitUrl;
        private readonly string gamePhotoNoSplitUrl;

        private bool imageLoading = false;

        public ImageSource Background { get; set; }
        
        public ImageSource LangImage
        {
            get
            {
                switch (Language)
                {
                    case LanguageEnum.Danish:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_dk.png"));
                    case LanguageEnum.Spanish:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_es.png"));
                    case LanguageEnum.Norwegian:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_no.png"));
                    case LanguageEnum.Swedish:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_se.png"));
                    default:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_en.png"));
                }
            }
        }
        public ImageSource RatingImage
        {
            get
            {
                switch (Rating)
                {
                    case 5:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_05.png"));
                    case 10:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_10.png"));
                    case 15:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_15.png"));
                    case 20:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_20.png"));
                    case 25:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_25.png"));
                    case 30:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_30.png"));
                    case 35:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_35.png"));
                    case 40:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_40.png"));
                    case 45:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_45.png"));
                    case 50:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_50.png"));
                    default:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Stars/star_00.png"));
                }
            }
        }
        public ImageSource TypeImage
        {
            get
            {
                switch (OWLS)
                {
                    case 5:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl1.png"));
                    case 10:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl2.png"));
                    case 15:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl3.png"));
                    case 20:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl4.png"));
                    case 25:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl5.png"));
                    case 30:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl6.png"));
                    case 35:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl7.png"));
                    case 40:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl8.png"));
                    default:
                        return new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Owls/owl0.png"));
                }
            }
        }

        public async void LoadImage()
        {
            if (imageLoading)
                return;
            imageLoading = true;

            LogicHelper.DownloadPhoto(creatorPhotoUrl, UpdateProfileImage);

            Background = await LogicHelper.GetBluredImage(gamePhotoNoSplitUrl);
            OnPropertyChanged(nameof(Background));
        }

        private void UpdateProfileImage(BitmapImage bitmap)
        {
            CreatorPhoto = bitmap;
            OnPropertyChanged(nameof(CreatorPhoto));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
