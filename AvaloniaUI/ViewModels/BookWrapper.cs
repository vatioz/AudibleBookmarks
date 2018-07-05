using AudibleBookmarks.Core.Models;
using Avalonia.Media;

namespace AvaloniaUI.ViewModels
{
    public class BookWrapper : Book
    {
        public ISolidColorBrush DownloadStatusBackground
        {
            get { return this.IsDownloaded ? Brushes.LightGreen : Brushes.LightPink; }
        }
    }
}
