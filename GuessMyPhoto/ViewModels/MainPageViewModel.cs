using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Game;

namespace GuessMyPhoto.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private LanguageEnum selectedLanguage;
        private CategoryEnum selectedCategory;
        private bool puzzlesDownloaded;
        private ObservableCollection<Puzzle> allPuzzles;

        #region SelectedLanguageProperties
        public bool? EnglishLang
        {
            get
            {
                if (selectedLanguage == LanguageEnum.English)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedLanguage = LanguageEnum.English;
                    AppInfo.AppUser.SelectedLanguage = selectedLanguage;
                }
            }
        }
        public bool? SpanishLang
        {
            get
            {
                if (selectedLanguage == LanguageEnum.Spanish)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedLanguage = LanguageEnum.Spanish;
                    AppInfo.AppUser.SelectedLanguage = selectedLanguage;
                }
            }
        }
        public bool? DanishLang
        {
            get
            {
                if (selectedLanguage == LanguageEnum.Danish)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedLanguage = LanguageEnum.Danish;
                    AppInfo.AppUser.SelectedLanguage = selectedLanguage;
                }
            }
        }
        public bool? NorwegianLang
        {
            get
            {
                if (selectedLanguage == LanguageEnum.Norwegian)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedLanguage = LanguageEnum.Norwegian;
                    AppInfo.AppUser.SelectedLanguage = selectedLanguage;
                }
            }
        }
        public bool? SwedishLang
        {
            get
            {
                if (selectedLanguage == LanguageEnum.Swedish)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedLanguage = LanguageEnum.Swedish;
                    AppInfo.AppUser.SelectedLanguage = selectedLanguage;
                }
            }
        }
        #endregion

        #region SelecredCategoryProperties
        public bool? MyFriendsSelected
        {
            get
            {
                if (selectedCategory == CategoryEnum.MyFriends)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedCategory = CategoryEnum.MyFriends;
                }
            }
        }
        public bool? AllPuzzlesSelected
        {
            get
            {
                if (selectedCategory == CategoryEnum.AllPuzzles)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedCategory = CategoryEnum.AllPuzzles;
                }
            }
        }
        public bool? ChallengesSelected
        {
            get
            {
                if (selectedCategory == CategoryEnum.Challenges)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedCategory = CategoryEnum.Challenges;
                }
            }
        }
        #endregion

        public LanguageEnum SelectedLanguage => selectedLanguage;
        public BitmapImage UserPhoto { get; set; }
        
        public bool ShowNoPuzzlesMessage => puzzlesDownloaded && allPuzzles.Count == 0 ;
        public bool PuzzlesDownloading => !puzzlesDownloaded;
        public ObservableCollection<Puzzle> Puzzles => allPuzzles;

        public MainPageViewModel()
        {
            
            selectedCategory = CategoryEnum.AllPuzzles;
            allPuzzles = new ObservableCollection<Puzzle>();
            if (AppInfo.AppUser != null)
            {
                selectedLanguage = AppInfo.AppUser.SelectedLanguage;
                LogicHelper.DownloadPhoto(LogicHelper.CreateImageUrl(ImageTypeToDownload.ProfilePhotoBig, AppInfo.AppUser.UserId), PhotoDownloaded);
            }
        }

        private void PhotoDownloaded(BitmapImage image)
        {
            UserPhoto = image;
            OnPropertyChanged(nameof(UserPhoto));
        }

        public async Task UpdatePuzzlesList()
        {
            allPuzzles.Clear();
            if (await LogicHelper.IsInternet())
            {
                puzzlesDownloaded = false;

                OnPropertyChanged(nameof(PuzzlesDownloading));

                GameTypes gamesType;
                if (selectedCategory == CategoryEnum.MyFriends)
                    gamesType = GameTypes.PuzzleListFromFriends;
                else if (selectedCategory == CategoryEnum.Challenges)
                    gamesType = GameTypes.ChallengeListAllCahllenges;
                else
                    gamesType = GameTypes.AllPuzzles;
                var result =
                    await Game.GetGameList(LogicHelper.GetLangString(selectedLanguage), GameSort.CreatedDesc, gamesType);
                foreach (var puzzle in result)
                {
                    allPuzzles.Add(puzzle);
                }
            }
            puzzlesDownloaded = true;
            OnPropertyChanged(nameof(PuzzlesDownloading));
            OnPropertyChanged(nameof(ShowNoPuzzlesMessage));
            LoadImage();
        }

        public void LoadImage(int itemCount = 0)
        {
            if (itemCount <= 0)
                itemCount = 10;
            itemCount = allPuzzles.Count > itemCount ? itemCount : allPuzzles.Count;
            for (int i = 0; i < itemCount; i++)
            {
                allPuzzles[i].LoadImage();
            }
        }
        //private void UpdateDisplayedPuzzles()
        //{
        //    //displayedPuzzles.Clear();
        //    //foreach (var item in allPuzzles.Where(r => r.Language == selectedLanguage))
        //    //{
        //    //    displayedPuzzles.Add(item);
        //    //}
        //    //OnPropertyChanged(nameof(Puzzles));
        //    //OnPropertyChanged(nameof(ShowNoPuzzlesMessage));
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
