using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Coding4Fun.Phone.Controls;
using WPRssReader.Resources;

namespace WPRssReader
{
    public partial class EditChannel : PhoneApplicationPage
    {
        List<string> _list = new List<string>();
        public EditChannel()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            for (var i = 1; i < App.ViewModel.Channels.Count + 1; i++)
            {
                _list.Add(i.ToString());
            }
            channelListPicker.ItemsSource = _list;
            channelListPicker.SelectedIndex = App.ViewModel.Channels.IndexOf(App.ViewModel.Channel);
        }

        private void SaveAndCloseClick(object sender, RoutedEventArgs e)
        {
            var title = channelName.Text;
            ToastPrompt toast = new ToastPrompt();
            toast.Foreground = App.WhiteColor;
            toast.MillisecondsUntilHidden = 1500;
            
            if (String.IsNullOrWhiteSpace(title)&&title.Length < 3)
            {
                toast.Message = AppResources.channel_title_error;
                toast.Show();
                return;
            }
       
            string address = channelLink.Text;
            if (String.IsNullOrWhiteSpace(address) || address.Equals("about:blank") ||
                !(address.Length > 7 && address.StartsWith("http://") ||
                  address.Length > 8 && address.StartsWith("https://")))
            {
                toast.Message = AppResources.add_message_error;
                toast.Show();
                return;
            }

            App.ViewModel.Channel.Title=title;
            App.ViewModel.Channel.URL=address;
            App.ViewModel.MoveChannel(App.ViewModel.Channel, channelListPicker.SelectedIndex);

            toast.Message = AppResources.channel_edit_done;
            toast.Show();

            NavigationService.GoBack();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}