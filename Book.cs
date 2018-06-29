using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AudibleBookmarks.Annotations;

namespace AudibleBookmarks
{
    public class Book : INotifyPropertyChanged
    {
        private IEnumerable<Chapter> _chapters;

        public Book()
        {
            Chapters = new List<Chapter>();
        }

        public string Asin { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Narrator { get; set; }
        public bool IsDownloaded { get; set; }

        public IEnumerable<Chapter> Chapters
        {
            get { return _chapters; }
            set
            {
                _chapters = value;
                OnPropertyChanged(nameof(RawLength));
                OnPropertyChanged(nameof(Length));
                OnPropertyChanged(nameof(FormattedLength));
            }
        }

        public long RawLength { get; set; }

        public TimeSpan Length => TimeSpan.FromTicks(RawLength);

        public string FormattedLength => $"{(int) Length.TotalHours}h {Length.Minutes}m";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}