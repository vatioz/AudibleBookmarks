using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;

namespace AudibleBookmarks.Core.Models
{
    public class Book : INotifyPropertyChanged
    {
        private const int MAX_SHORTTITLE_LENGTH = 15;
        private static ILog _logger = LogManager.GetLogger(typeof(Book));

        private List<Chapter> _chapters;
        private ObservableCollection<Bookmark> _bookmarks;

        public Book()
        {
            Chapters = new List<Chapter>();
            Bookmarks = new ObservableCollection<Bookmark>();

        }

        public string Asin { get; set; }

        public string ShortTitle
        {
            get
            {
                var titleLength = Title.Length;
                if (titleLength > MAX_SHORTTITLE_LENGTH)
                {
                    return Title.Substring(0, MAX_SHORTTITLE_LENGTH);
                }

                return Title;
            }
        }

        public string Title { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Narrators { get; set; }

        public string AuthorLabel => $"A: {string.Join(", ", Authors)}";
        public string NarratorLabel => $"N: {string.Join(", ", Narrators)}";

        /// <summary>
        /// 0 - Not downloaded
        /// 1 - Queued
        /// 2 - Download paused
        /// 3 - Download in progress
        /// 4 - ?
        /// 5 - Downloaded
        /// </summary>
        public long DownloadState { get; set; }
        public bool IsDownloaded => DownloadState == 5; 

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
            var foundChapter = Chapters.LastOrDefault(ch => ch.StartTime < position);
            if (foundChapter == null)
            {
                _logger.Error($"Couldn't find chapter for position {position} in book {ShortTitle}. ");
                return new Chapter{ Title = "Chapter", StartTime = 0, Duration = RawLength };
            }

            return foundChapter;
        }

        public override string ToString()
        {
            return $"Book Asin: {Asin}, Title: {Title}, AuthorLabel: {AuthorLabel}, NarratorLabel: {NarratorLabel}, DownloadState: {DownloadState}, IsDownloaded: {IsDownloaded}, Length: {FormattedLength}, # of chapters: {Chapters.Count}, # of bookmarks: {Bookmarks.Count}";
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}