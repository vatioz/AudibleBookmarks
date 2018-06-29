using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using AudibleBookmarks.Annotations;
using TinyMessenger;

namespace AudibleBookmarks
{
    public class MainViewModel
    {
        // tODO Add counts - total bookmarks in book, empty, notes only, titles only
        // TODO Add inteligent DB seek (auto load)
        // TODO Add some sort of template for export
        // TODO Add option to export without empties
        // TODO make UI resizable
        // TODO think about how to display notes (ellipsis..., max lines, max height, max width?)
        // TODO add logic to CanExportExecute
        // TODO write README.MD for github
        // TODO look why there are multiple books for courses

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
            }
        }

        private void LoadChapters(Book selectedBook)
        {
            Chapters.Clear();
            if (selectedBook == null)
                return;

            string sql = $"select StartTime, Name, Duration From Chapters Where Asin = '{selectedBook.Asin}'";
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var ch = new Chapter
                {
                    Title = reader["Name"] as string,
                    Duration = reader["Duration"] as long?,
                    StartTime = reader["StartTime"] as long?
                };
                Chapters.Add(ch);
            }

            SelectedBook.Chapters = Chapters;
        }

        private void LoadBookmarks(Book selectedBook)
        {
            Bookmarks.Clear();
            if (selectedBook == null)
                return;

            string sql = $"select Position, StartPosition, Note, Title, LastModifiedTime From Bookmarks Where Asin = '{selectedBook.Asin}'";
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var position = reader["Position"] as long?;
                Bookmarks.Add(new Bookmark
                {
                    Note = reader["Note"] as string,
                    Title = reader["Title"] as string,
                    Modified = reader["LastModifiedTime"] as DateTime?,
                    End = position,
                    Start = reader["StartPosition"] as long?,
                    Chapter = GetChapter(position)
                });
            }
        }

        private Chapter GetChapter(long? position)
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
            return true;
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
            string sql = "select b.Asin, b.Title, b.Duration, b.FileName, ba.Author, bn.Narrator from Books b JOIN BookAuthors ba ON b.Asin = ba.Asin JOIN BookNarrators bn ON b.Asin = bn.Asin";
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Books.Add(new Book
                {
                    Asin = reader["Asin"] as string,
                    Title = reader["Title"] as string,
                    Author = reader["Author"] as string,
                    Narrator = reader["Narrator"] as string,
                    IsDownloaded = (reader["FileName"] as string) != null,
                    RawLength = reader["Duration"] as long?
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
            }
        }

        public long? RawLength { get; set; }

        public TimeSpan Length
        {
            get
            {
                if (RawLength.HasValue)
                    return TimeSpan.FromTicks(RawLength.Value);
                else
                    return TimeSpan.Zero;
            }
        }

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
        public long? Start { get; set; }
        public long? End { get; set; }
        public TimeSpan PositionOverall
        {
            get
            {
                if (End.HasValue)
                    return TimeSpan.FromTicks(End.Value);
                else
                    return TimeSpan.Zero;
            }
        }
        public TimeSpan PositionChapter
        {
            get
            {
                if (End.HasValue && Chapter.StartTime.HasValue)
                {
                    var positionInChapter = End.Value - Chapter.StartTime.Value;
                    return TimeSpan.FromTicks(positionInChapter);
                }
                else
                    return TimeSpan.Zero;
            }
        }
        public Chapter Chapter { get; set; }
        public DateTime? Modified { get; set; }
        public bool IsEmptyBookmark
        {
            get
            {
                return string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Note);
            }
        }
    }

    public class Chapter
    {
        public string Title { get; set; }
        public long? StartTime { get; set; }
        public long? Duration { get; set; }
    }
}
