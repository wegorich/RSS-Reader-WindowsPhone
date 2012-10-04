namespace WPRssReader
{
    public partial class Setting
    {
        private readonly int[] _itemsCount = {25, 50, 75, 100};

        public Setting()
        {
            InitializeComponent();
            DataContext = App.Settings;
            listPicker.ItemsSource = _itemsCount;
        }

        private void LeaveFeedbackClick(object sender, System.Windows.RoutedEventArgs e)
        {
            App.LeaveFeedback();
        }
    }
}