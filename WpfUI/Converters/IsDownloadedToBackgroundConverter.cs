﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudibleBookmarks.Converters
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
}
