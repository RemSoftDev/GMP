using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Game;

namespace GuessMyPhoto.ViewModels
{
    public class AddNewGameViewModel:INotifyPropertyChanged
    {
        char[] englishAlphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        char[] danishAndNorwegianAlphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'Æ', 'Ø', 'Å' };
        char[] spanishAlphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'Ñ' };
        char[] swedishAlphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'Å', 'Ä', 'Ö' };

        private bool wordEntered = false;
        private bool photoMaked = false;
        private bool languageSelected = false;
        private Puzzle puzzle;
        private WriteableBitmap photo;
        private ObservableCollection<Cell> enteredWord;
        private ObservableCollection<Cell> resultWord;
        private ObservableCollection<Cell> scrambledWord;

        public Puzzle PuzzleData => puzzle;
        public ObservableCollection<Cell> EnteredWord => enteredWord;
        public ObservableCollection<Cell> ResultWord => resultWord;
        public ObservableCollection<Cell> ScrambledWord => scrambledWord;
        public bool NeedDisplaySelectLangControl => photoMaked && !languageSelected;
        public bool NeedDisplayEnterWordControl => languageSelected && !wordEntered;
        public bool NeedDisplayScrambledWordControl => wordEntered;
        public bool NeedDisplayFinalyControl { get; set; }
        public bool CanGoToScrambling => enteredWord.Count(r => r.Letter != "") >= 3;
        public ImageSource UserPhoto { get; set; }
        public ImageSource LanguageImage { get; set; }
        public PictureSourceEnum PictureSource { get; set; }
        public ImageSource BackgroundPhoto { get; set; }
        public WriteableBitmap Photo
        {
            get { return photo; }
            set
            {
                if (value != null)
                {
                    photo = value;
                    OnPropertyChanged(nameof(Photo));
                }
            }
        }

        public AddNewGameViewModel(PictureSourceEnum selectedPictureSource)
        {
            scrambledWord = new ObservableCollection<Cell>();
            resultWord = new ObservableCollection<Cell>();
            enteredWord = new ObservableCollection<Cell>();
            enteredWord.Add(new Cell
            {
                BackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDA, 0x95, 0x00)),
                Letter = ""
            });
            for (int i = 0; i < 7; i++)
            {
                enteredWord.Add(new Cell
                {
                    Letter = "",
                    BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xA8, 0xA8, 0xA8)),
                    ForeColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                });
            }
            puzzle = new Puzzle
            {
                CreatorName = AppInfo.AppUser.Name,
                Language = AppInfo.AppUser.SelectedLanguage
            };
            LanguageImage = new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_empty.png"));
            PictureSource = selectedPictureSource;
            LogicHelper.DownloadPhoto(LogicHelper.CreateImageUrl(ImageTypeToDownload.ProfilePhotoBig, AppInfo.AppUser.UserId), PhotoDownloaded);
        }
        private void PhotoDownloaded(BitmapImage image)
        {
            UserPhoto = image;
            OnPropertyChanged(nameof(UserPhoto));
        }
        public void KeyPressed(char key)
        {
            if (key == 8 /*Backspace*/)
            {
                BackKeyPressed();
                return;
            }
            if (key == 13 /*Enter*/)
            {
                GoToScrambling();
            }
            string[] lettersArray = { "Æ", "Ø", "Å", "Ñ", "Ö", "Ä", "Å" };
            var pressedLetter = key.ToString().ToUpper();
            if ((key >= 'A' && key <= 'z') || lettersArray.Any(r => r == key.ToString().ToUpper()))
            {
                var letter = enteredWord.FirstOrDefault(r => r.Letter == "");
                if (letter != null)
                {
                    letter.Letter = pressedLetter;
                    letter.ForeColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    letter.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xAE, 0x00));

                    OnPropertyChanged(nameof(CanGoToScrambling));
                }
                var nextEmptyCell = enteredWord.FirstOrDefault(r => r.Letter == "");
                if (nextEmptyCell != null)
                {
                    nextEmptyCell.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDA, 0x95, 0x00));
                }
            }
            
        }

        public void BackKeyPressed()
        {
            var letter = enteredWord.LastOrDefault(r => r.Letter != "");
            if (letter != null)
            {
                var index = enteredWord.IndexOf(letter);
                letter.Letter = "";
                letter.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDA, 0x95, 0x00));
                if (index >= 0 && index < enteredWord.Count - 1)
                    enteredWord[index + 1].BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xA8, 0xA8, 0xA8));
                
                OnPropertyChanged(nameof(CanGoToScrambling));
            }
        }

        public void GoToScrambling()
        {
            wordEntered = true;
            OnPropertyChanged(nameof(NeedDisplayEnterWordControl));
            OnPropertyChanged(nameof(NeedDisplayScrambledWordControl));

            puzzle.Word = string.Empty;
            resultWord.Clear();
            foreach (var item in enteredWord)
            {
                if (item.Letter == "")
                    continue;
                var copy = new Cell();
                copy.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xE4, 0xE4, 0xE4));
                copy.ForeColor = new SolidColorBrush(Color.FromArgb(0xff, 0x47, 0x47, 0x47));
                copy.Letter = item.Letter;
                resultWord.Add(copy);

                puzzle.Word += copy.Letter;
            }
            Scramble();
        }
        public void GoToEnterWord(LanguageEnum language)
        {
            puzzle.Language = language;
            LanguageImage = puzzle.LangImage;
            languageSelected = true;
            wordEntered = false;
            OnPropertyChanged(nameof(NeedDisplayEnterWordControl));
            OnPropertyChanged(nameof(NeedDisplayScrambledWordControl));
            OnPropertyChanged(nameof(CanGoToScrambling));
            OnPropertyChanged(nameof(LanguageImage));
        }

        public void GoToLanguageSelection()
        {
            photoMaked = true;
            languageSelected = false;
            OnPropertyChanged(nameof(NeedDisplayEnterWordControl));
            OnPropertyChanged(nameof(NeedDisplaySelectLangControl));
        }
        public void GoToPhotoSelection()
        {
            photoMaked = false;
            LanguageImage = new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/Languages/language_empty.png"));
            OnPropertyChanged(nameof(NeedDisplaySelectLangControl));
            OnPropertyChanged(nameof(LanguageImage));
        }

        public async Task<GameCreateResponseModel> PostPuzzle()
        {
            GameCreateResponseModel result;
            try
            {
                result = await Game.CreateGame(new GameModel
                {
                    CreatorUid = AppInfo.AppUser.UserId,
                    CreateGameType = "A",
                    Language = LogicHelper.GetLangString(puzzle.Language),
                    Word = puzzle.Word,
                    Photo = this.Photo
                });
                if (result.Status == GameCreateStatus.Ok)
                {
                    AppInfo.AppUser.TotalCreatedPuzzles++;
                    wordEntered = false;
                    NeedDisplayFinalyControl = true;
                    OnPropertyChanged(nameof(NeedDisplayScrambledWordControl));
                    OnPropertyChanged(nameof(NeedDisplayFinalyControl));
                }
            }
            catch
            {
                result = new GameCreateResponseModel();
                result.Status = GameCreateStatus.PostError;
                result.GameId = String.Empty;
            }
            
            return result;
        }

        public void Scramble()
        {
            scrambledWord.Clear();
            Random rnd = new Random();
            var needGenerateLetterCount = 8 - puzzle.Word.Length;
            var fullLLengthStr = puzzle.Word;
            if (needGenerateLetterCount > 0)
            {
                char[] alphabet;
                switch (puzzle.Language)
                {
                    case LanguageEnum.Danish:
                    case LanguageEnum.Norwegian:
                        alphabet = danishAndNorwegianAlphabet;
                        break;
                    case LanguageEnum.Spanish:
                        alphabet = spanishAlphabet;
                        break;
                    case LanguageEnum.Swedish:
                        alphabet = swedishAlphabet;
                        break;
                    default:
                        alphabet = englishAlphabet;
                        break;
                }
                for (int i = 0; i < needGenerateLetterCount; i++)
                {
                    fullLLengthStr += alphabet[rnd.Next(alphabet.Length)];
                }
            }
            int index;
            int sequenceNumber = 0;
            fullLLengthStr = fullLLengthStr.ToUpper();
            while (fullLLengthStr.Length > 0)
            {
                index = rnd.Next(fullLLengthStr.Length);
                scrambledWord.Add(new Cell
                {
                    Letter = fullLLengthStr[index].ToString(),
                    BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xE4, 0xE4, 0xE4)),
                    ForeColor = new SolidColorBrush(Color.FromArgb(0xff, 0x47, 0x47, 0x47)),
                    Id = sequenceNumber
                });
                fullLLengthStr = fullLLengthStr.Remove(index, 1);
                sequenceNumber++;
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
