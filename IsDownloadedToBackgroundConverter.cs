using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudibleBookmarks
{
    public class IsDownloadedToBackgroundConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDownloaded = (bool)value;

            return isDownloaded ? Brushes.LightGreen : Brushes.LightSlateGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BookmarkStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEmptyBookmark = (bool)value;

            return isEmptyBookmark ? Brushes.PaleVioletRed : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
