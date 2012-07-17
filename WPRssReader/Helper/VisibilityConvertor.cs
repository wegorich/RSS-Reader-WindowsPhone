using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPRssReader.Helper
{
    public class VisibilityConvertor : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (Visibility))
            {
                if (value is bool)
                {
                    return ((bool) value) ? Visibility.Visible : Visibility.Collapsed;
                }
                if (value is int)
                {
                    return ((int) value == 0) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}