using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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


        // TODO make template of app of this sort with all the necessary starting points - TinyMessenger, RelayCommand, FileDialogService, MainViewModel, ListBox


        private string _pathToLibrary;
        private DatabaseService _dbService;
        private Book _selectedBook;

        public string WinowTitle => $"Audible Bookmarks [{Version}]";

        public string Version => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
            .ProductVersion;

        public ObservableCollection<Book> Books { get; set; }
        public ObservableCollection<Bookmark> Bookmarks { get; set; }

        public Book SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                _selectedBook = value;
                if (_selectedBook != null)
                {
                    LoadChapters(_selectedBook);
                    LoadBookmarks(_selectedBook);

                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalBookmarkCount));
                OnPropertyChanged(nameof(EmptyBookmarkCount));
                OnPropertyChanged(nameof(OnlyTitleBookmarkCount));
                OnPropertyChanged(nameof(OnlyNoteBookmarkCount));
            }
        }

        public MainViewModel()
        {
            var svc = new FileDialogService();
            svc.StartListening();
            _dbService = new DatabaseService();

            Books = new ObservableCollection<Book>();
            Bookmarks = new ObservableCollection<Bookmark>();

            FilterableBooks = CollectionViewSource.GetDefaultView(Books);
            FilterableBooks.Filter = FilterBooks;
            FilterableBookmarks = CollectionViewSource.GetDefaultView(Bookmarks);
            FilterableBookmarks.Filter = FilterBookmarks;

            //if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //    FileOpened(@"C:\Users\Petr\AppData\Local\Packages\AudibleInc.AudibleforWindowsPhone_xns73kv1ymhp2\LocalState\library - Copy.db");

        }

        #region | Stats

        public int TotalBookCount => Books.Count;
        public int DownloadedBookCount => Books.Count(b => b.IsDownloaded);
        public int TotalBookmarkCount => SelectedBook?.Bookmarks.Count ?? 0;
        public int EmptyBookmarkCount => SelectedBook?.Bookmarks.Count(bm => bm.IsEmptyBookmark) ?? 0;
        public int OnlyTitleBookmarkCount => SelectedBook?.Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Note) && !string.IsNullOrWhiteSpace(bm.Title)) ?? 0;
        public int OnlyNoteBookmarkCount => SelectedBook?.Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Title) && !string.IsNullOrWhiteSpace(bm.Note)) ?? 0;

        #endregion

        #region | Commands

        public ICommand LoadAudibleLibrary { get { return new RelayCommand(CanLoadAudibleLibraryExecute, LoadAudibleLibraryExecute); } }

        private bool CanLoadAudibleLibraryExecute()
        {
            return true;
        }

        private void LoadAudibleLibraryExecute()
        {
            TinyMessengerHub.Instance.Publish(new OpenFileMessage(this, FileOpened));

        }

        public ICommand Export { get { return new RelayCommand(CanExportExecute, ExportExecute); } }

        private bool CanExportExecute()
        {
            return SelectedBook != null && SelectedBook.Bookmarks.Count > 0;
        }

        private void ExportExecute()
        {
            var sb = new StringBuilder();
            foreach (var bookmark in SelectedBook.Bookmarks)
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

        #endregion

        #region | Filtering stuff

        public ICollectionView FilterableBooks { get; }
        public ICollectionView FilterableBookmarks { get; set; }

        private string _bookmarkFilterValue;
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
            FilterableBookmarks.Refresh();
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
        
        private string _bookFilterValue;
        public string BookFilterValue
        {
            get { return _bookFilterValue; }
            set
            {
                _bookFilterValue = value;
                UpdateBookFilter();
            }
        }
        private void UpdateBookFilter()
        {
            FilterableBooks.Refresh();
        }
        private bool FilterBooks(object item)
        {
            if (string.IsNullOrWhiteSpace(BookFilterValue))
                return true;

            var book = item as Book;
            return book.Title.ToUpper().Contains(BookFilterValue.ToUpper());
        }

        #endregion

        #region | Refresh stuff on book selection

        private void FileOpened(string path)
        {
            //_pathToLibrary = path;
            _dbService.OpenSqliteConnection(path);
            LoadBooks();
        }

        private void LoadBooks()
        {
            Books.Clear();
            var books = _dbService.GetBooks();
            foreach (var book in books)
            {
                Books.Add(book);
            }
        }

        private void LoadChapters(Book selectedBook)
        {
            _dbService.LoadChapters(selectedBook);
        }

        private void LoadBookmarks(Book selectedBook)
        {
            _dbService.LoadBookmarks(selectedBook);

            // update filterable collection view source
            Bookmarks.Clear();
            foreach (var bookmark in selectedBook.Bookmarks)
            {
                Bookmarks.Add(bookmark);
            }
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
