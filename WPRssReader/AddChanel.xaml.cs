using System;
using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WPRssReader.Resources;

namespace WPRssReader
{
    public partial class AddChanel
    {
        public AddChanel()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            //cos ApplicationBarIconButton doesn`t have binding at all
            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).Text = AppResources.add_save;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).Text = AppResources.add_search;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[2]).Text = AppResources.add_cancel;
        }

        private void SaveClick(object sender, EventArgs e)
        {
            string address = RssLink.Text;
            if (String.IsNullOrEmpty(address) || address.Equals("about:blank") ||
                !(address.Length > 7 && address.StartsWith("http://") ||
                  address.Length > 8 && address.StartsWith("https://")))
            {
                MessageBox.Show(AppResources.add_message_error);
                return;
            }
            try
            {
                App.ViewModel.AddChannelCommand.DoExecute("http://bashorg.org/rss.xml");//address);
                MessageBox.Show(AppResources.add_message);
            }
            catch (Exception)
            {
                MessageBox.Show(AppResources.add_message_error);
            }
        }

        private void CancelClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void WebClick(object sender, EventArgs e)
        {
            var web = new WebBrowserTask();
            web.Show();
        }
    }
}