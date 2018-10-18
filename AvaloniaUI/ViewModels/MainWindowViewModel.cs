using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Models;
using AudibleBookmarks.Core.Services;
using AudibleBookmarks.Core.Utils;
using AvaloniaUI.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        // TODO make version sharedbetween code and publish script
        // tODO GridSplitter
        // TODO book details section
        // TODO add log4net

        private DatabaseService _dbService;
        private BookWrapper _selectedBook;


        public bool Test = true;
        public string WinowTitle => TitleProvider.GetTitleWithVersion();

        public ReactiveList<BookWrapper> Books { get; set; }
        public IReactiveDerivedList<BookWrapper> FilterableBooks { get; set; }
        public ReactiveList<Bookmark> Bookmarks { get; set; }
        public IReactiveDerivedList<Bookmark> FilterableBookmarks { get; set; }


        public BookWrapper SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedBook, value);

                
                    //this.RaisePropertyChanged(nameof(TotalBookmarkCount));
                    //this.RaisePropertyChanged(nameof(EmptyBookmarkCount));
                    //this.RaisePropertyChanged(nameof(OnlyTitleBookmarkCount));
                    //this.RaisePropertyChanged(nameof(OnlyNoteBookmarkCount));
            }
        }

        public MainWindowViewModel()
        {
            ISubscribable fileSvc = new FileDialogService();
            fileSvc.StartListening();
            ISubscribable alertSvc = new AlertService();
            alertSvc.StartListening();

            _dbService = new DatabaseService();

            Books = new ReactiveList<BookWrapper>();
            Bookmarks = new ReactiveList<Bookmark>();
            FilterableBooks = Books.CreateDerivedCollection(b=>b, FilterBooks);
            FilterableBookmarks = Bookmarks.CreateDerivedCollection(b=>b, FilterBookmarks);

            var observableSelectedBook = this.WhenAnyValue(x => x.SelectedBook);
            observableSelectedBook.Subscribe(b => 
            {
                if (b == null)
                    return;
                LoadChapters(b);
                LoadBookmarks(b);
            });

            _totalBookmarkCount = this.WhenAny(
                x => x.SelectedBook, 
                x => x.Sender.Bookmarks.Count()
                )
                .ToProperty(this, x=>x.TotalBookmarkCount);

            _emptyBookmarkCount = this.WhenAny(
                x => x.SelectedBook,
                x => x.Sender.Bookmarks.Count(bm => bm.IsEmptyBookmark)
                )
                .ToProperty(this, x => x.EmptyBookmarkCount);

            _onlyTitleBookmarkCount = this.WhenAny(
                x => x.SelectedBook,
                x => x.Sender.Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Note) && !string.IsNullOrWhiteSpace(bm.Title))
                )
                .ToProperty(this, x => x.OnlyTitleBookmarkCount);

            _onlyNoteBookmarkCount = this.WhenAny(
                x => x.SelectedBook,
                x => x.Sender.Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Title) && !string.IsNullOrWhiteSpace(bm.Note))
                )
                .ToProperty(this, x => x.OnlyNoteBookmarkCount);


            FileOpened(PathHelper.TryToGuessPathToLibrary());
        }



        #region | Stats

        public int TotalBookCount => Books.Count;
        public int DownloadedBookCount { get { return Books.Count(b => b.IsDownloaded); } }

        readonly ObservableAsPropertyHelper<int> _totalBookmarkCount;
        public int TotalBookmarkCount => _totalBookmarkCount.Value;
        //SelectedBook?.Bookmarks.Count ?? 0;

        readonly ObservableAsPropertyHelper<int> _emptyBookmarkCount;
        public int EmptyBookmarkCount => _emptyBookmarkCount.Value;

        readonly ObservableAsPropertyHelper<int> _onlyTitleBookmarkCount;
        public int OnlyTitleBookmarkCount => _onlyTitleBookmarkCount.Value;
            //SelectedBook?.Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Note) && !string.IsNullOrWhiteSpace(bm.Title)) ?? 0;

        readonly ObservableAsPropertyHelper<int> _onlyNoteBookmarkCount;
        public int OnlyNoteBookmarkCount => _onlyNoteBookmarkCount.Value;
            //SelectedBook?.Bookmarks.Count(bm => string.IsNullOrWhiteSpace(bm.Title) && !string.IsNullOrWhiteSpace(bm.Note)) ?? 0;

        #endregion

        #region | Commands

        

        private void LoadAudibleLibrary()
        {
            TinyMessengerHub.Instance.Publish(new OpenFileMessage(this, FileOpened));

        }

        private bool CanExportExecute()
        {
            return SelectedBook != null && SelectedBook.Bookmarks.Count > 0;
        }

        private void ExportBookmarks()
        {

            try
            {
                var sb = BuildExportString();
                TinyMessengerHub.Instance.Publish(new SaveFileMessage(this, stream => { SaveFile(stream, sb); }));
            }
            catch (Exception ex)
            {
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
            var template = File.ReadAllText("BookmarkTemplate.txt");
            var sb = new StringBuilder();
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
            return sb;
        }

        #endregion

        #region | Filtering stuff

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
            FilterableBookmarks.Reset();
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
            return title.ToUpper().Contains(BookmarkFilterValue.ToUpper()) || 
                note.ToUpper().Contains(BookmarkFilterValue.ToUpper()) ||
                bookmark.PositionVisualization.Contains(BookmarkFilterValue)
                ;
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
            FilterableBooks.Reset();
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

        private void FileOpened(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            _dbService.OpenSqliteConnection(path);
            LoadBooks();
        }

        private void LoadBooks()
        {
            Books.Clear();
            var books = _dbService.GetBooks();
            foreach (var book in books)
            {
                var bookWrap = new BookWrapper
                {
                    Asin = book.Asin,
                    Authors = book.Authors,
                    Bookmarks = book.Bookmarks,
                    Chapters = book.Chapters,
                    IsDownloaded = book.IsDownloaded,
                    Narrators = book.Narrators,
                    RawLength = book.RawLength,
                    Title = book.Title
                };
                Books.Add(bookWrap);
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

        private void PublishException(Exception ex)
        {
            TinyMessengerHub.Instance.Publish(new GenericTinyMessage<Exception>(this, ex));
        }
    }
}
