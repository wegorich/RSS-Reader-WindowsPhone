using System;
using System.Globalization;
using System.Windows.Data;

namespace WPRssReader.Helper
{
    public class BoolToOpacityConvertor : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (double) &&
                value is bool)
            {
                return (bool) value ? 0.35d : 1d;
            }
            if (targetType == typeof (double) &&
                (value is int))
            {
                return !System.Convert.ToBoolean(value) ? 0.5d : 1d;
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