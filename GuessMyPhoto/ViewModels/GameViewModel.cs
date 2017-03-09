using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Contacts;
using GuessMyPhoto.Models.Game;
using Microsoft.Graphics.Canvas.UI.Xaml;
using GuessMyPhoto.Models.User;

namespace GuessMyPhoto.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private Puzzle puzzle;
        private ObservableCollection<Cell> lettersArray;
        private ObservableCollection<Cell> resultWord;
        private ObservableCollection<ContactsStore> contactsCollection;
        private BitmapImage nextPhoto;
        private bool timeOut = false;
        private bool skiped = false;
        private bool win = false;
        private int rating = 0;
        private DispatcherTimer timer;
        private int timeInSeconds = 3;
        private int timeInMillieconds = 3000;

        public Puzzle PuzzleData => puzzle;
        public ObservableCollection<Cell> LettersArray => lettersArray;
        public ObservableCollection<Cell> ResultWord => resultWord;
        public ObservableCollection<ContactsStore> ContactsCollection => contactsCollection;

        public string SkipButtonText
        {
            get
            {
                if (skiped || timeOut || win)
                    return "Done";
                return "Skip";
            }
        }

        public string EndGameMessage
        {
            get
            {
                if (skiped)
                    return "Skipped!";
                if (timeOut)
                    return "Time out!";
                return "Completed!";
            }
        }
        public int TimeInMilliseconds => timeInMillieconds;
        public int TimeInSeconds => timeInSeconds;
        public int ReportTypeIndex { get; set; }
        public bool NeedDisplayStartTimer => timeInSeconds > 0;
        public bool NeedDisplayPlayControl => !NeedDisplayStartTimer && !timeOut && !skiped && !win;
        public bool NeedDisplayEndGameControl => timeOut || skiped || win;
        public bool NeedDisplayReportControl { get; set; }
        public bool NeedDisplayCallengeControl { get; set; }
        public bool CanSkip => (!NeedDisplayStartTimer && !(skiped || timeOut || win)) || ((skiped || timeOut || win) && rating > 0);
        public bool CanGoBack => (skiped || timeOut || win) && rating > 0;
        public ImageSource Image { get; set; }

        public GameViewModel(Puzzle puzzle)
        {
            this.puzzle = puzzle;
            contactsCollection = new ObservableCollection<ContactsStore>();
        }

        public void StartNow()
        {
            timeInSeconds = 0;
            StartGame();
        }

        public void Skip()
        {
            skiped = true;
            EndGame();
        }

        public void Report()
        {
            NeedDisplayReportControl = true;
            OnPropertyChanged(nameof(NeedDisplayReportControl));
        }
        public void CancelReport()
        {
            NeedDisplayReportControl = false;
            OnPropertyChanged(nameof(NeedDisplayReportControl));
        }

        public async Task<GameReportStatus> SendReport()
        {
            return await Game.ReportGame(puzzle.GameId, (GameReportTypes) ReportTypeIndex);
        }

        public async Task<bool> Initialize()
        {
            var result = await Game.StartGame(puzzle.GameId);
            if (result.Status != GameStartStatus.Ok)
            {
                var dialog = new MessageDialog("You have already played this game or an error occurred. Please select another one.", "Can't start this game");
                await dialog.ShowAsync();
                return false;
            }
            //Image = puzzle.Background;
            //OnPropertyChanged(nameof(Image));
            puzzle.Word = result.WordToGuess;

            resultWord = new ObservableCollection<Cell>();
            for (int i = 0; i < puzzle.Word.Length; i++)
            {
                resultWord.Add(new Cell
                {
                    Letter = "",
                    BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xA8, 0xA8, 0xA8)),
                    ForeColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                });
            }
            lettersArray = new ObservableCollection<Cell>();


            CreateLettersArray(result.Leters);

            App.IgnorGoBackRequest = true;

            timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
            timer.Tick += Timer_Tick;
            timer.Start();

            LogicHelper.DownloadPhoto(puzzle.GamePhotoMaxSplitUrl, NextPhotoDownloaded);

            OnPropertyChanged(nameof(ResultWord));
            OnPropertyChanged(nameof(LettersArray));
            return true;
        }

        private void NextPhotoDownloaded(BitmapImage bitmap)
        {
            nextPhoto = bitmap;
        }
        private void Timer_Tick(object sender, object e)
        {

            timeInSeconds--;
            if (timeInSeconds <= 0)
            {
                StartGame();
            }
            else
                OnPropertyChanged(nameof(TimeInSeconds));
        }
        private void GameTimer_Tick(object sender, object e)
        {
            timeInMillieconds -= 10;
            if (timeInMillieconds == 2000)
            {
                this.Image = nextPhoto;
                OnPropertyChanged(nameof(Image));
                LogicHelper.DownloadPhoto(puzzle.GamePhotoNoSplitUrl, NextPhotoDownloaded);
            }
            else if (timeInMillieconds == 1000)
            {
                this.Image = nextPhoto;
                OnPropertyChanged(nameof(Image));
            }
            else if (timeInMillieconds <= 0)
            {
                timeInMillieconds = 0;
                TimeOut();
            }
            OnPropertyChanged(nameof(TimeInMilliseconds));
        }

        private void TimeOut()
        {
            timeOut = true;
            EndGame();
        }

        private void StartGame()
        {
            timer.Stop();
            this.Image = nextPhoto;
            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(NeedDisplayStartTimer));
            OnPropertyChanged(nameof(NeedDisplayPlayControl));
            OnPropertyChanged(nameof(CanSkip));
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 80) };
            timer.Tick += GameTimer_Tick;
            timer.Start();
            LogicHelper.DownloadPhoto(puzzle.GamePhotoMidSplitUrl, NextPhotoDownloaded);
        }

        private void EndGame()
        {
            timer.Stop();

            if (timeInMillieconds > 1000)
            {
                LogicHelper.DownloadPhoto(puzzle.GamePhotoNoSplitUrl, image =>
                {
                    this.Image = image;
                    OnPropertyChanged(nameof(Image));
                } );
                
            }

            resultWord.Clear();
            var str = puzzle.Word.ToUpper();
            foreach (char t in str)
            {
                resultWord.Add(new Cell
                {
                    Letter = t.ToString(),
                    BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xE4, 0xE4, 0xE4)),
                    ForeColor = new SolidColorBrush(Color.FromArgb(0xff, 0x47, 0x47, 0x47)),
                });
            }
            OnPropertyChanged(nameof(SkipButtonText));
            OnPropertyChanged(nameof(CanSkip));
            OnPropertyChanged(nameof(NeedDisplayPlayControl));
            OnPropertyChanged(nameof(NeedDisplayEndGameControl));
            OnPropertyChanged(nameof(EndGameMessage));
            if (skiped)
                timeInMillieconds = 0;
            Game.EndGame(puzzle.GameId, timeInMillieconds);
        }

        private void CreateLettersArray(string letters)
        {
            int sequenceNumber = 0;
            letters = letters.ToUpper();
            foreach (var letter in letters)
            {
                lettersArray.Add(new Cell
                {
                    Letter = letter.ToString(),
                    BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xE4, 0xE4, 0xE4)),
                    ForeColor = new SolidColorBrush(Color.FromArgb(0xff, 0x47, 0x47, 0x47)),
                    Id = sequenceNumber
                });
                sequenceNumber++;
            }
        }

        public void LetterInArrayTapped(Cell letterFromArray)
        {
            if (letterFromArray.IsUsed)
                return;
            var letterInWord = resultWord.FirstOrDefault(r => r.Letter == "");
            if (letterInWord != null)
            {
                letterInWord.Letter = letterFromArray.Letter;
                letterInWord.ForeColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                letterInWord.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xAE, 0x00));
                letterInWord.Id = letterFromArray.Id;

                letterFromArray.BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                letterFromArray.ForeColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                letterFromArray.IsUsed = true;
            }
            if (resultWord.All(r => r.Letter != ""))
            {
                string word = string.Empty;
                foreach (var cell in ResultWord)
                {
                    word += cell.Letter;
                }
                if (word == puzzle.Word.ToUpper())
                {
                    win = true;
                    EndGame();
                }
            }
        }
        public void LetterInResultWordTapped(Cell letterInWord)
        {
            var letterFromArray = lettersArray.FirstOrDefault(r => r.Id == letterInWord.Id);
            if (letterFromArray != null)
            {
                letterInWord.Letter = "";
                letterInWord.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xA8, 0xA8, 0xA8));

                letterFromArray.BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xE4, 0xE4, 0xE4));
                letterFromArray.ForeColor = new SolidColorBrush(Color.FromArgb(0xff, 0x47, 0x47, 0x47));
                letterFromArray.IsUsed = false;
            }
        }

        public async Task PuzzleRated(int rating)
        {
            this.rating = rating;
            OnPropertyChanged(nameof(CanSkip));
            App.IgnorGoBackRequest = false;
            var reult = await Game.SetStars(puzzle.GameId, this.rating);
        }

        public async Task GoToChallenge()
        {
            NeedDisplayCallengeControl = true;
            OnPropertyChanged(nameof(NeedDisplayCallengeControl));
            contactsCollection.Clear();
            bool isFirst = true;

            if (AppInfo.AppUser?.FbId != null && AppInfo.AppUser.FbId != "no_email_accept" && AppInfo.AppUser.FbId != "no_facebook")
            {
                contactsCollection.Add(new ContactsStore
                {
                    Name = "",
                    IsFirst = isFirst,
                    ContactsData = new ObservableCollection<ContactData>(new[]
                    {
                        new ContactData
                        {
                            ContactStr = "Challenge on Facebook",
                            FontSize = 22,
                            Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x10, 0xB0)),
                            Icon = new BitmapImage(new Uri(@"ms-appx:///Assets/Graphics/FacebookIcon.png"))
                        }
                    })
                });
                isFirst = false;
            }

            var contactStore = await ContactManager.RequestStoreAsync();
            var contacts = await contactStore.FindContactsAsync();
            foreach (var contact in contacts)
            {
                var contactsList = new ObservableCollection<Models.Contacts.ContactData>();
                foreach (var email in contact.Emails)
                {
                    contactsList.Add(new Models.Contacts.ContactData
                    {
                        ContactStr = email.Address,
                        FontSize = 17,
                        Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x59, 0x59, 59))
                    });
                }
                foreach (var phone in contact.Phones)
                {
                    contactsList.Add(new Models.Contacts.ContactData
                    {
                        ContactStr = phone.Number,
                        FontSize = 17,
                        Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x59, 0x59, 59))
                    });
                }

                if (contactsList.Count > 0)
                {
                    BitmapImage image = new BitmapImage();
                    if (contact.SmallDisplayPicture != null)
                        image.SetSource(await contact.SmallDisplayPicture.OpenReadAsync());
                    else
                        image.UriSource = new Uri(@"ms-appx:///Assets/Graphics/profile_picture.png");
                    contactsCollection.Add(new ContactsStore
                    {
                        ContactsData = contactsList,
                        Name = contact.FirstName + " " + contact.LastName,
                        Icon = image,
                        IsFirst = isFirst
                    });
                }
                isFirst = false;
            }
        }

        public void HideChallengeControl()
        {
            NeedDisplayCallengeControl = false;
            OnPropertyChanged(nameof(NeedDisplayCallengeControl));
        }

        public async Task<ChallengeCreateResponseModel> SendChallenges(IList<object> selectedItems)
        {
            List<string> contacts = new List<string>();
            foreach (var item in selectedItems)
            {
                var contact = item as ContactData;
                if (contact != null)
                {
                    if (contact.ContactStr != "Challenge on Facebook")
                        contacts.Add(contact.ContactStr.Replace("+", ""));
                    else
                    {
                        FacebookHelper fbh = new FacebookHelper();
                        await fbh.CreateFbPost();
                    }
                }
            }
            if(contacts.Count == 0)
            {
                return new ChallengeCreateResponseModel { Status = CreateChallengeStatus.Ok };
            }
            return await Game.CreateChallenge(puzzle.GameId, contacts);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
