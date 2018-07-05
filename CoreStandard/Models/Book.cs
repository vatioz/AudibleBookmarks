using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AudibleBookmarks.Core.Models
{
    public class Book : INotifyPropertyChanged
    {
        private List<Chapter> _chapters;
        private ObservableCollection<Bookmark> _bookmarks;

        public Book()
        {
            Chapters = new List<Chapter>();
            Bookmarks = new ObservableCollection<Bookmark>();

        }

        public string Asin { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Narrators { get; set; }

        public string AuthorLabel => $"A: {string.Join(", ", Authors)}";
        public string NarratorLabel => $"N: {string.Join(", ", Narrators)}";
        public bool IsDownloaded { get; set; }

        public List<Chapter> Chapters
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

        public ObservableCollection<Bookmark> Bookmarks
        {
            get { return _bookmarks; }
            set
            {
                _bookmarks = value;
                OnPropertyChanged();
            }
        }

        public long RawLength { get; set; }

        public TimeSpan Length => TimeSpan.FromTicks(RawLength);

        public string FormattedLength => $"{(int)Length.TotalHours}h {Length.Minutes}m";


        public Chapter GetChapter(long position)
        {
            return Chapters.Last(ch => ch.StartTime < position);
        }      




        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}