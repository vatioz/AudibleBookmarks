using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using AudibleBookmarks.Annotations;
using TinyMessenger;

namespace AudibleBookmarks
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // TODO Add inteligent DB seek (auto load)
        // TODO Add some sort of template for export
        // TODO Add option to export without empties
        // TODO think about how to display notes (ellipsis..., max lines, max height, max width?)
        // TODO write README.MD for github
        // TODO look why there are multiple books for courses
        // TODO make code more aware of various exception-states (add try catches)
        // TODO do refactor


        // TODO make template of app of this sort with all the necessary starting points - TinyMessenger, RelayCommand, FileDialogService, MainViewModel, ListBox


        private string _pathToLibrary;
        private SQLiteConnection _connection;
        private Book _selectedBook;

        private ICollectionView _filterableBooks;
        private ICollectionView _filterableBookmarks;

        public ICollectionView FilterableBooks
        {
            get { return _filterableBooks; }
        }

        public ICollectionView FilterableBookmarks
        {
            get { return _filterableBookmarks; }
        }

        private string _bookmarkFilterValue;
        private string _bookFilterValue;


        public ObservableCollection<Book> Books { get; set; }
        public ObservableCollection<Bookmark> Bookmarks { get; set; }
        public ObservableCollection<Chapter> Chapters { get; set; }

        public Book SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                _selectedBook = value;
                LoadChapters(_selectedBook);
                LoadBookmarks(_selectedBook);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalBookmarkCount));
                OnPropertyChanged(nameof(EmptyBookmarkCount));
                OnPropertyChanged(nameof(OnlyTitleBookmarkCount));
                OnPropertyChanged(nameof(OnlyNoteBookmarkCount));
            }
        }

        public int TotalBookCount => Books.Count;

        public int DownloadedBookCount => Books.Count(b => b.IsDownloaded);



        public int TotalBookmarkCount => Bookmarks.Count;
        public int EmptyBookmarkCount => Bookmarks.Count(bm => bm.IsEmptyBookmark);
        public int OnlyTitleBookmarkCount => Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Note) && !string.IsNullOrWhiteSpace(bm.Title));
        public int OnlyNoteBookmarkCount => Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Title) && !string.IsNullOrWhiteSpace(bm.Note));


        private void LoadChapters(Book selectedBook)
        {
            Chapters.Clear();
            if (selectedBook == null || _connection == null)
                return;

            string sql = $"select StartTime, Name, Duration From Chapters Where Asin = '{selectedBook.Asin}'";
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var ch = new Chapter
                {
                    Title = reader["Name"] as string,
                    Duration = (long)reader["Duration"],
                    StartTime = (long)reader["StartTime"]
                };
                Chapters.Add(ch);
            }

            SelectedBook.Chapters = Chapters;
        }

        private void LoadBookmarks(Book selectedBook)
        {
            Bookmarks.Clear();
            if (selectedBook == null || _connection == null)
                return;

            string sql = $"select Position, StartPosition, Note, Title, LastModifiedTime From Bookmarks Where Asin = '{selectedBook.Asin}'";
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var position = (long)reader["Position"];
                Bookmarks.Add(new Bookmark
                {
                    Note = reader["Note"] as string,
                    Title = reader["Title"] as string,
                    Modified = (DateTime)reader["LastModifiedTime"],
                    End = position,
                    Start = (long)reader["StartPosition"],
                    Chapter = GetChapter(position)
                });
            }
        }

        private Chapter GetChapter(long position)
        {
            return Chapters.Last(ch => ch.StartTime < position);
        }

        public ICommand LoadAudibleLibrary { get { return new RelayCommand(CanLoadAudibleLibraryExecute, LoadAudibleLibraryExecute); } }
        public ICommand Export { get { return new RelayCommand(CanExportExecute, ExportExecute); } }

        private void ExportExecute()
        {
            var sb = new StringBuilder();
            foreach (var bookmark in Bookmarks)
            {
                if (bookmark.IsEmptyBookmark)
                    continue;

                sb.AppendLine(bookmark.Chapter.Title);
                if (!string.IsNullOrWhiteSpace(bookmark.Title))
                    sb.AppendLine(bookmark.Title);
                if (!string.IsNullOrWhiteSpace(bookmark.Note))
                    sb.AppendLine(bookmark.Note);
            }

            var dlg = new SaveFileDialog();
            dlg.Filter = "Text Files (*.txt)|*.txt";
            dlg.DefaultExt = "txt";
            dlg.AddExtension = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var stream = dlg.OpenFile();
                var sw = new StreamWriter(stream);
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
            }
        }

        private bool CanExportExecute()
        {
            return Bookmarks.Count > 0;
        }

        public MainViewModel()
        {
            var svc = new FileDialogService();
            svc.StartListening();

            Books = new ObservableCollection<Book>();
            Bookmarks = new ObservableCollection<Bookmark>();
            Chapters = new ObservableCollection<Chapter>();

            _filterableBooks = CollectionViewSource.GetDefaultView(Books);
            _filterableBooks.Filter = FilterBooks;

            _filterableBookmarks = CollectionViewSource.GetDefaultView(Bookmarks);
            _filterableBookmarks.Filter = FilterBookmarks;

            Books.Add(new Book
            {
                Title = "Title"
            });
            Books.Add(new Book
            {
                Title = "Title"
            });
            Books.Add(new Book
            {
                Title = "Title",
                IsDownloaded = true
            });
            Books.Add(new Book
            {
                Title = "Title"
            });



            Bookmarks.Add(new Bookmark
            {
                Title = "Bm",
                Note = "Note"
            });
            Bookmarks.Add(new Bookmark
            {
                Title = "Bm",
                Note = "Note"
            });
            Bookmarks.Add(new Bookmark
            {
                Title = "",
                Note = ""
            });
            Bookmarks.Add(new Bookmark
            {
                Title = "Bm",
                Note = "Note"
            });


            SelectedBook = Books[0];

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                FileOpened(@"C:\Users\Petr\AppData\Local\Packages\AudibleInc.AudibleforWindowsPhone_xns73kv1ymhp2\LocalState\library - Copy.db");

        }

        private bool FilterBooks(object item)
        {
            if (string.IsNullOrWhiteSpace(BookFilterValue))
                return true;

            var book = item as Book;
            return book.Title.ToUpper().Contains(BookFilterValue.ToUpper());
        }

        private bool FilterBookmarks(object item)
        {
            if (string.IsNullOrWhiteSpace(BookmarkFilterValue))
                return true;

            var bookmark = item as Bookmark;
            var title = bookmark.Title ?? string.Empty;
            var note = bookmark.Note ?? string.Empty;
            return title.ToUpper().Contains(BookmarkFilterValue.ToUpper()) || note.ToUpper().Contains(BookmarkFilterValue.ToUpper());
        }

        public string BookmarkFilterValue
        {
            get { return _bookmarkFilterValue; }
            set
            {
                _bookmarkFilterValue = value;
                UpdateBookmarkFilter();
            }
        }

        private void UpdateBookmarkFilter()
        {
            _filterableBookmarks.Refresh();
        }

        private void UpdateBookFilter()
        {
            _filterableBooks.Refresh();
        }

        public string BookFilterValue
        {
            get { return _bookFilterValue; }
            set
            {
                _bookFilterValue = value;
                UpdateBookFilter();
            }
        }

        private void LoadAudibleLibraryExecute()
        {
            TinyMessengerHub.Instance.Publish(new OpenFileMessage(this, FileOpened));

        }

        private void FileOpened(string path)
        {
            _pathToLibrary = path;
            OpenSqliteConnection();
            LoadBooks();
        }

        private void LoadBooks()
        {
            Books.Clear();

            var authorDictionary = new Dictionary<string, List<string>>();
            string sqlAuthors = "select Asin, Author from BookAuthors";
            SQLiteCommand commandAuthors = new SQLiteCommand(sqlAuthors, _connection);
            SQLiteDataReader readerAuthors = commandAuthors.ExecuteReader();
            while (readerAuthors.Read())
            {
                var asin = (string)readerAuthors["Asin"];
                var author = (string)readerAuthors["Author"];
                if (authorDictionary.ContainsKey(asin))
                    authorDictionary[asin].Add(author);
                else
                    authorDictionary.Add(asin, new List<string> { author });
            }


            var narratorDictionary = new Dictionary<string, List<string>>();
            string sqlNarrators = "select Asin, Narrator from BookNarrators";
            SQLiteCommand commandNarrators = new SQLiteCommand(sqlNarrators, _connection);
            SQLiteDataReader readerNarrators = commandNarrators.ExecuteReader();
            while (readerNarrators.Read())
            {
                var asin = (string)readerNarrators["Asin"];
                var narrator = (string)readerNarrators["Narrator"];
                if (narratorDictionary.ContainsKey(asin))
                    narratorDictionary[asin].Add(narrator);
                else
                    narratorDictionary.Add(asin, new List<string> { narrator });
            }






            string sql = "select b.Asin, b.Title, b.Duration, b.FileName from Books b";
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var asin = (string)reader["Asin"];
                var authors = authorDictionary.ContainsKey(asin) ? authorDictionary[asin] : Enumerable.Empty<string>();
                var narrators = narratorDictionary.ContainsKey(asin) ? narratorDictionary[asin] : Enumerable.Empty<string>();

                Books.Add(new Book
                {
                    Asin = asin,
                    Title = (string)reader["Title"],
                    IsDownloaded = (reader["FileName"] as string) != null,
                    RawLength = (long)reader["Duration"],
                    Author = string.Join(", ", authors),
                    Narrator = string.Join(", ", narrators)
                });
            }


        }

        private void OpenSqliteConnection()
        {
            _connection = new SQLiteConnection($"Data Source={_pathToLibrary};Version=3;");
            _connection.Open();
        }

        private bool CanLoadAudibleLibraryExecute()
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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

    public class Bookmark
    {
        public string Title { get; set; }
        public string Note { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public TimeSpan PositionOverall => TimeSpan.FromTicks(End);

        public TimeSpan PositionChapter
        {
            get
            {
                var positionInChapter = End - Chapter.StartTime;
                return TimeSpan.FromTicks(positionInChapter);
            }
        }
        public Chapter Chapter { get; set; }
        public DateTime Modified { get; set; }
        public bool IsEmptyBookmark => string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Note);
    }

    public class Chapter
    {
        public string Title { get; set; }
        public long StartTime { get; set; }
        public long Duration { get; set; }
    }
}
