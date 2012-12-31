using System;
using System.Globalization;
using System.Windows.Data;
using WPRssReader.Resources;

namespace WPRssReader.Helper
{
    public class DateConvertor : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (string) &&
                value is DateTime?)
            {
                var dat = (DateTime?) value;
                if (parameter == null)
                {
                    return dat.Value.ToString("dd MMMM yyyy HH:mm", CultureInfo.CurrentUICulture);
                }


                switch (dat.Value.Year)
                {
                    default:
                        return String.Format(AppResources.date_last_update,
                                             dat.Value.ToString("dd MMMM yyyy HH:mm", CultureInfo.CurrentUICulture));
                    case 1900:
                        return AppResources.date_updating;
                    case 1901:
                        return AppResources.date_fail;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ToUserFriendlyString(object value)
        {
            return Convert(value, typeof(string), null, CultureInfo.CurrentCulture);
        }
        #endregion
    }
}