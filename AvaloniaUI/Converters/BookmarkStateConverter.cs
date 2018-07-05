using System;
using System.Globalization;
using Avalonia.Markup;
using Avalonia.Media;

namespace AvaloniaUI.Converters
{
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