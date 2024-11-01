using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ButchersCashier.Models
{
    public class ExcelFile : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string FileName { get; set; }
        public string FilePath { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}