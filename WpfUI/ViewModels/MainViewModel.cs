using AudibleBookmarks.About;
using AudibleBookmarks.Annotations;
using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Models;
using AudibleBookmarks.Core.Services;
using AudibleBookmarks.Core.Utils;
using AudibleBookmarks.Services;
using AudibleBookmarks.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AudibleBookmarks.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // TODO think about how to display notes (ellipsis..., max lines, max height, max width?)
        // TODO add about
        // TODO add help
        // tODO icon?

        // TODO make template of app of this sort with all the necessary starting points - TinyMessenger, RelayCommand, FileDialogService, MainViewModel, ListBox

        private static ILog _logger = LogManager.GetLogger(typeof(MainViewModel));

        private string _pathToLibrary;
        public string PathToLibrary
        {
            get { return _pathToLibrary; }
            set
            {
                _pathToLibrary = value;
                OnPropertyChanged();
            }
        }

        private DatabaseService _dbService;
        private Book _selectedBook;

        public string WinowTitle => TitleProvider.GetTitleWithVersion();
        
        public ObservableCollection<Book> Books { get; set; }
        public ObservableCollection<Bookmark> Bookmarks { get; set; }

        public Book SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                _selectedBook = value;
                _logger.Info($"Book selection changed.");
                if (_selectedBook != null && _selectedBook.IsDownloaded)
                {
                    _logger.Info($"Selected book {_selectedBook}.");
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
            _logger.Info($"Starting AudibleBookmarks {TitleProvider.GetTitleWithVersion()}");
            ISubscribable fileSvc = new FileDialogService();
            fileSvc.StartListening();
            ISubscribable alertSvc = new AlertService();
            alertSvc.StartListening();

            _dbService = new DatabaseService();

            Books = new ObservableCollection<Book>();
            Bookmarks = new ObservableCollection<Bookmark>();

            FilterableBooks = CollectionViewSource.GetDefaultView(Books);
            FilterableBooks.Filter = FilterBooks;
            FilterableBookmarks = CollectionViewSource.GetDefaultView(Bookmarks);
            FilterableBookmarks.Filter = FilterBookmarks;

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                FileOpened(PathHelper.TryToGuessPathToLibrary());
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

        public ICommand About => new RelayCommand(()=> true, AboutExecute);

        private void AboutExecute()
        {
            About.About about = new About.About();
            about.DataContext = new AboutInfo();
            about.ShowDialog();
            //AboutDialog.ShowWindow(new AboutInfo());
        }

        public ICommand LoadAudibleLibrary { get { return new RelayCommand(CanLoadAudibleLibraryExecute, LoadAudibleLibraryExecute); } }

        private bool CanLoadAudibleLibraryExecute()
        {
            return true;
        }

        private void LoadAudibleLibraryExecute()
        {
            TinyMessengerHub.Instance.Publish(new OpenFileMessage(this, FileOpened));

        }

        private void FileOpened(string path)
        {
            PathToLibrary = path;

            _logger.Info($"Attempting toload DB from file {path}");

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            _dbService.OpenSqliteConnection(path);
            LoadBooks();
        }

        public ICommand Export { get { return new RelayCommand(CanExportExecute, ExportExecute); } }

        private bool CanExportExecute()
        {
            return SelectedBook != null && SelectedBook.Bookmarks.Count > 0;
        }

        private void ExportExecute()
        {
            _logger.Info($"ExportExecute()");
            try
            {
                var sb = BuildExportString();
                TinyMessengerHub.Instance.Publish(new SaveFileMessage(this, stream => { SaveFile(stream, sb); }));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error exporting bookmarks for {SelectedBook?.Title}. Ex: {ex}");
                PublishException(ex);
                return;
            }
        }

        private void SaveFile(Stream stream, StringBuilder stringBuilder)
        {
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(stringBuilder.ToString());
                sw.Flush();
            }
        }

        private StringBuilder BuildExportString()
        {
            _logger.Info($"Loading bookmark template.");
            var template = File.ReadAllText("BookmarkTemplate.txt");
            _logger.Info($"Bookmark template loaded: \n{template}");
            var sb = new StringBuilder();

            _logger.Info($"Building text to export for {SelectedBook.Title} from total of {SelectedBook.Bookmarks.Count}");
            foreach (var bookmark in SelectedBook.Bookmarks)
            {
                if (bookmark.IsEmptyBookmark)
                    continue;

                var propDictionary = new Dictionary<string, object>();
                propDictionary.Add(nameof(Bookmark.Title), bookmark.Title);
                propDictionary.Add(nameof(Bookmark.Note), bookmark.Note);
                propDictionary.Add(nameof(Bookmark.PositionChapter), bookmark.PositionChapter);
                propDictionary.Add(nameof(Bookmark.PositionOverall), bookmark.PositionOverall);
                propDictionary.Add("ChapterTitle", bookmark.Chapter.Title);


                var populatedTemplate = template.Inject(propDictionary);
                sb.AppendLine(populatedTemplate);
            }
            _logger.Info($"Built text is {sb.Length} characters long");
            return sb;
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
            if (bookmark == null)
                return true; // just a safeguard, rather leave weird stuff on display

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
            if (book == null)
                return true; // just a safeguard, rather leave weird stuff on display

            return book.Title.ToUpper().Contains(BookFilterValue.ToUpper());
        }

        #endregion

        #region | Refresh stuff on book selection

        

        private void LoadBooks()
        {
            Books.Clear();
            var books = _dbService.GetBooks();
            foreach (var book in books)
            {
                Books.Add(book);
            }
            _logger.Info($"Loaded {Books.Count} books");
        }

        private void LoadChapters(Book selectedBook)
        {
            _dbService.LoadChapters(selectedBook);
            _logger.Info($"{selectedBook.Title}: Loaded {selectedBook.Chapters.Count} chapters");
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
            _logger.Info($"{selectedBook.Title}: Loaded {selectedBook.Bookmarks.Count} bookmarks");
        }

        #endregion

        private void PublishException(Exception ex)
        {
            TinyMessengerHub.Instance.Publish(new GenericTinyMessage<Exception>(this, ex));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
