using System;
using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WPRssReader.Resources;
using Coding4Fun.Phone.Controls;
using System.Windows.Input;

namespace WPRssReader
{
    public partial class AddChanel
    {
        public AddChanel()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            //cos ApplicationBarIconButton doesn`t have binding at all
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.add_save;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.add_search;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = AppResources.add_cancel;
        }

        private void UrlKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveClick(sender, e);
                this.Focus();
            }
        }  

        private void SaveClick(object sender, EventArgs e)
        {
            string address = RssLink.Text;

            if (String.IsNullOrWhiteSpace(address) || address.Equals("about:blank") ||
                !(address.Length > 7 && address.StartsWith("http://") ||
                  address.Length > 8 && address.StartsWith("https://")))
            {
                ToastPrompt toast = new ToastPrompt();
                toast.Foreground = App.WhiteColor;
                toast.MillisecondsUntilHidden = 1500;
                toast.Message = AppResources.add_message_error;
                toast.Show();
                return;
            }
            try
            {
                App.ViewModel.AddChannelCommand.DoExecute(address);
                ToastPrompt toast = new ToastPrompt();
                toast.Foreground = App.WhiteColor;
                toast.MillisecondsUntilHidden = 1500;
                toast.Message = AppResources.add_message;
                toast.Show();
            }
            catch (Exception)
            {
                ToastPrompt toast = new ToastPrompt();
                toast.MillisecondsUntilHidden = 1500;
                toast.Message = AppResources.add_message_error;
                toast.Foreground = App.WhiteColor;
                toast.Show();
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