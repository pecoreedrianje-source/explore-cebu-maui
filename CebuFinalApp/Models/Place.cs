using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CebuFinalApp.Models
{
    public class Place : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Rating { get; set; }

        private bool _isFavorite = false;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite == value) return;
                _isFavorite = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HeartColor)); // ✅ Updates button color via binding
            }
        }

        public string HeartColor => IsFavorite ? "#E53E3E" : "#CBD5E0";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}