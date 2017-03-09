using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using GuessMyPhoto.Annotations;

namespace GuessMyPhoto.Models.Game
{
    public class Cell : INotifyPropertyChanged
    {
        private string letter = string.Empty;
        private Brush backgroundColor;
        private Brush foreColor;

        public int Id { get; set; }
        public bool IsUsed { get; set; }

        public string Letter
        {
            get { return letter; }
            set
            {
                letter = value;
                OnPropertyChanged(nameof(Letter));
            }
        }

        public Brush BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }
        public Brush ForeColor
        {
            get { return foreColor; }
            set
            {
                foreColor = value;
                OnPropertyChanged(nameof(ForeColor));
            }
        }

        public Cell GetCopy()
        {
            return this.MemberwiseClone() as Cell;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
