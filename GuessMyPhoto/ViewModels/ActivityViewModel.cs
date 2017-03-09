using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Game;

namespace GuessMyPhoto.ViewModels
{
    public class ActivityViewModel : INotifyPropertyChanged
    {

        private ActivityCategoryEnum selectedCategory;
        private bool puzzlesDownloaded;
        public BitmapImage profilePhoto;
        private ObservableCollection<Puzzle> allPuzzles;

        #region SelecredCategoryProperties
        public bool? PlayedSelected
        {
            get
            {
                if (selectedCategory == ActivityCategoryEnum.Played)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedCategory = ActivityCategoryEnum.Played;
                }
            }
        }
        public bool? CreatedSelected
        {
            get
            {
                if (selectedCategory == ActivityCategoryEnum.Created)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedCategory = ActivityCategoryEnum.Created;
                }
            }
        }
        public bool? ChallengesSelected
        {
            get
            {
                if (selectedCategory == ActivityCategoryEnum.Challenges)
                    return true;
                return false;
            }
            set
            {
                if (value.HasValue && value.Value)
                {
                    selectedCategory = ActivityCategoryEnum.Challenges;
                }
            }
        }
        #endregion

        public bool ShowNoPuzzlesMessage => puzzlesDownloaded && allPuzzles.Count == 0;
        public bool PuzzlesDownloading => !puzzlesDownloaded;
        public ObservableCollection<Puzzle> Puzzles => allPuzzles;

        public ActivityViewModel(BitmapImage profilePhoto)
        {
            this.profilePhoto = profilePhoto;
            selectedCategory = ActivityCategoryEnum.Created;
            allPuzzles = new ObservableCollection<Puzzle>();
        }
        public async Task UpdatePuzzlesList()
        {
            //this.profilePhoto = 
            allPuzzles.Clear();
            if (await LogicHelper.IsInternet())
            {
                puzzlesDownloaded = false;
                OnPropertyChanged(nameof(PuzzlesDownloading));

                GameTypes gamesType;
                if (selectedCategory == ActivityCategoryEnum.Created)
                    gamesType = GameTypes.MyPuzzleList;
                else if (selectedCategory == ActivityCategoryEnum.Played)
                    gamesType = GameTypes.MyPlayedPuzzleList;
                else
                    gamesType = GameTypes.MyCallengesList;
                var result = await Game.GetGameList("all", GameSort.CreatedDesc, gamesType);
                foreach (var puzzle in result)
                {
                    //puzzle.CreatorPhoto = profilePhoto;
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
