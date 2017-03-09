using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GuessMyPhoto.Annotations;
using GuessMyPhoto.Enums;
using GuessMyPhoto.Models.Contacts;
using GuessMyPhoto.Models.Game;
using GuessMyPhoto.Models.User;

namespace GuessMyPhoto.ViewModels
{
    public class GameInfoViewModel : INotifyPropertyChanged
    {
        private Puzzle puzzle;
        private ObservableCollection<ContactsStore> contactsCollection;
        private ObservableCollection<Cell> resultWord;
        private bool createdByCurrentUser;
        private bool challenge;

        public bool NeedEmptyListTextBox { get; set; }
        public string EmptyListText { get; set; }

        public bool NeedDisplayPlayerListControl { get; set; }
        public bool NeedDisplayCallengeControl { get; set; }
        public bool NeedDisplayEndGameControl { get; set; }
        public Puzzle PuzzleData => puzzle;
        public ObservableCollection<ContactsStore> ContactsCollection => contactsCollection;
        public ObservableCollection<Cell> ResultWord => resultWord;
        public List<PlayerModel> Players { get; set; }
        public ImageSource Image { get; set; }
        public ImageSource Background { get; set; }

        public string PostOrChallengeBtnText => createdByCurrentUser ? "Post on FB" : "Challenge";

        public GameInfoViewModel(Puzzle puzzle, bool createdByCurrentUser, bool challenge)
        {
            this.puzzle = puzzle;
            this.challenge = challenge;
            this.createdByCurrentUser = createdByCurrentUser;
            contactsCollection = new ObservableCollection<ContactsStore>();
            NeedDisplayEndGameControl = true;
        }
        public async Task<bool> Initialize()
        {
            //var result = await Game.StartGame(puzzle.GameId);
            //if (result.Status != GameStartStatus.Ok)
            //{
            //    var dialog = new MessageDialog("You have already played this game or an error occurred. Please select another one.", "Can't start this game");
            //    await dialog.ShowAsync();
            //    return false;
            //}
            //Image = puzzle.Background;
            //OnPropertyChanged(nameof(Image));
            //puzzle.Word = result.WordToGuess;
            if (string.IsNullOrWhiteSpace(puzzle?.Word))
                return false;
            Background = await LogicHelper.GetBluredImage(puzzle.GamePhotoNoSplitUrl);
            OnPropertyChanged(nameof(Background));
            resultWord = new ObservableCollection<Cell>();
            for (int i = 0; i < puzzle.Word.Length; i++)
            {
                resultWord.Add(new Cell
                {
                    Letter = puzzle.Word[i].ToString(),
                    BackgroundColor = new SolidColorBrush(Color.FromArgb(0xff, 0xE4, 0xE4, 0xE4)),
                    ForeColor = new SolidColorBrush(Color.FromArgb(0xff, 0x47, 0x47, 0x47)),
                });
            }
            LogicHelper.DownloadPhoto(puzzle.GamePhotoNoSplitUrl, PhotoDownloaded);
            

            OnPropertyChanged(nameof(ResultWord));
            return true;
        }
        private void PhotoDownloaded(BitmapImage bitmap)
        {
            Image = bitmap;
            OnPropertyChanged(nameof(Image));
        }

        public async Task GoToShareOrChallenge()
        {
            if (createdByCurrentUser)
            {
                FacebookHelper fb = new FacebookHelper();
                await fb.ShareOnFb();
            }
            else
            {
                await GoToChallenge();
            }
        }
        private async Task GoToChallenge()
        {
            NeedDisplayEndGameControl = false;
            OnPropertyChanged(nameof(NeedDisplayEndGameControl));
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
            NeedDisplayEndGameControl = true;
            OnPropertyChanged(nameof(NeedDisplayEndGameControl));
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
            if (contacts.Count == 0)
            {
                return new ChallengeCreateResponseModel { Status = CreateChallengeStatus.Ok };
            }
            return await Game.CreateChallenge(puzzle.GameId, contacts);
        }

        public async Task GoToPlayersList()
        {
            Players = new List<PlayerModel>();
            NeedDisplayEndGameControl = false;
            NeedDisplayPlayerListControl = true;
            OnPropertyChanged(nameof(NeedDisplayEndGameControl));
            OnPropertyChanged(nameof(NeedDisplayPlayerListControl));
            var result = await Game.GetPlayersList(puzzle.GameId, AppInfo.AppUser.UserId, challenge?PlayerListType.InChallenge : PlayerListType.InGame);
            if (result != null && result.Count > 0)
            {
                foreach (var item in result)
                {
                    if (item != null)
                        Players.Add(item);
                }
                OnPropertyChanged(nameof(Players));
            }
            else if(result.Count == 0)
            {
                NeedEmptyListTextBox = true;
                OnPropertyChanged(nameof(NeedEmptyListTextBox));
                EmptyListText = "No Players";
                OnPropertyChanged(nameof(EmptyListText));
            }
            else
            {
                var dialog = new MessageDialog("An error occurred when try to get players list. Please try again later.");
                await dialog.ShowAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HidePlayersListControl()
        {
            Players = null;
            NeedDisplayEndGameControl = true;
            NeedDisplayPlayerListControl = false;
            NeedEmptyListTextBox = false ;
            OnPropertyChanged(nameof(NeedEmptyListTextBox));
            OnPropertyChanged(nameof(NeedDisplayEndGameControl));
            OnPropertyChanged(nameof(NeedDisplayPlayerListControl));
        }
    }
}
