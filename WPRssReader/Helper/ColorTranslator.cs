using System;
using System.Windows.Media;

namespace WPRssReader.Helper
{
    public static class ColorTranslator
    {
        public static string ToHTML(this Color color)
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}", color.R,
                                 color.G,
                                 color.B);
        }
    }
}