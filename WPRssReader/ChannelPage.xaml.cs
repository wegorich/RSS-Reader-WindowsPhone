using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using WPRssReader.Model;
using WPRssReader.Resources;

namespace WPRssReader
{
    public partial class ChannelPage
    {
        private readonly ApplicationBarIconButton _next;
        private readonly ApplicationBarIconButton _preview;

        public ChannelPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            _preview = (ApplicationBarIconButton) ApplicationBar.Buttons[0];
            _next = (ApplicationBarIconButton) ApplicationBar.Buttons[2];
            //cos ApplicationBarIconButton doesn`t have binding at all
            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).Text = AppResources.preview;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).Text = AppResources.check_all;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[2]).Text = AppResources.next;

            MenuItemVisibility();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!NavigationContext.QueryString.ContainsKey("ID")) return;

            int id;
            int.TryParse(NavigationContext.QueryString["ID"], out id);

            App.ViewModel.Channel = App.ViewModel.Channels.FirstOrDefault(x => x.ID == id);
            App.ViewModel.UpdateChannel(App.ViewModel.Channel);
        }

        private void ArticleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            App.ViewModel.Article = (Article) e.AddedItems[0];
            NavigationService.Navigate(new Uri("/RssPage.xaml?val=channel", UriKind.Relative));
            ((ListBox) sender).SelectedItem = null;
        }

        private void CheckAllClick(object sender, EventArgs e)
        {
            foreach (Article art in App.ViewModel.Channel.Articles.Where(x => x.IsRead == false))
            {
                art.IsRead = true;
            }
        }

        private void NextChannelClick(object sender, EventArgs e)
        {
            int number = App.ViewModel.Channels.IndexOf(App.ViewModel.Channel) + 1;

            App.ViewModel.Channel = App.ViewModel.Channels.ElementAt(number);
            MenuItemVisibility();
        }

        private void PreviewChannelClick(object sender, EventArgs e)
        {
            int number = App.ViewModel.Channels.IndexOf(App.ViewModel.Channel) - 1;

            App.ViewModel.Channel = App.ViewModel.Channels.ElementAt(number);
            MenuItemVisibility();
        }

        private void MenuItemVisibility()
        {
            if (App.ViewModel.Channel != null)
            {
                _next.IsEnabled = App.ViewModel.Channels.IndexOf(App.ViewModel.Channel) !=
                                  App.ViewModel.Channels.Count - 1;
                _preview.IsEnabled = App.ViewModel.Channels.IndexOf(App.ViewModel.Channel) != 0;
            }
            else
            {
                _next.IsEnabled =
                    _preview.IsEnabled = false;
            }
        }
    }
}