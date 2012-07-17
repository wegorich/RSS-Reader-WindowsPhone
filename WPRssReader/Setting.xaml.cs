using Microsoft.Phone.Controls;

namespace WPRssReader
{
    public partial class Setting : PhoneApplicationPage
    {
        private readonly int[] _itemsCount = {25, 50, 75, 100};

        public Setting()
        {
            InitializeComponent();
            DataContext = App.Settings;
            listPicker.ItemsSource = _itemsCount;
        }
    }
}