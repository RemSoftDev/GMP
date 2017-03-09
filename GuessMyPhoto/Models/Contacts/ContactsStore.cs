using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml.Media;

namespace GuessMyPhoto.Models.Contacts
{
    public class ContactsStore
    {
        public string Name { get; set; }
        public bool IsFirst { get; set; }
        public ImageSource Icon { get; set; }
        public ObservableCollection<ContactData> ContactsData { get; set; }
    }

    public class ContactData
    {
        //public ObservableCollection<string> ContactsData { get; set; }
        public string ContactStr { get; set; }
        public SolidColorBrush Foreground { get; set; }
        public double FontSize { get; set; }
        public ImageSource Icon { get; set; }
    }

    //public class PhoneContact
    //{
    //    public string Name { get; set; }
    //    public List<string> Phone { get; set; }
    //}
}
