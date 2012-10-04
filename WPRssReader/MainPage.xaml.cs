using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WPRssReader.Model;
using WPRssReader.Resources;

namespace WPRssReader
{
    public partial class MainPage
    {
        private readonly Dictionary<object, Action<Channel>> _action =
            new Dictionary<object, Action<Channel>>();

        private readonly Dictionary<int, string> _appBars =
            new Dictionary<int, string>();

        private readonly Dictionary<object, Action> _loadAction =
            new Dictionary<object, Action>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            _action.Add("DELETE", App.ViewModel.DeleteChannel);
            _action.Add("START_MENU", PinChannelToStart);
            _action.Add("UP", App.ViewModel.MoveChannelUp);
            _action.Add("DOWN", App.ViewModel.MoveChannelDown);

            _loadAction.Add("NEW", App.ViewModel.LoadNextNewArticles);
            _loadAction.Add("ALL", App.ViewModel.LoadNextAllArticles);
            _loadAction.Add("STARED", App.ViewModel.LoadNextStaredArticles);

            _loadAction.Add(0, App.ViewModel.DoAddNewArticles);
            _loadAction.Add(1, App.ViewModel.RefreshAll);
            _loadAction.Add(2, App.ViewModel.RefreshArticles);
            _loadAction.Add(3, App.ViewModel.RefreshStared);

            _loadAction.Add(11, App.ViewModel.ReadAll);
            _loadAction.Add(12, App.ViewModel.ReadNew);
            _loadAction.Add(13, () =>
                {
                    foreach (Article art in App.ViewModel.StaredArticles)
                    {
                        art.IsStared = false;
                    }
                    App.ViewModel.RefreshStared();
                });

            _appBars.Add(0, "channelBar");
            _appBars.Add(1, "allBar");
            _appBars.Add(2, "newBar");
            _appBars.Add(3, "staredBar");
            //cos ApplicationBarIconButton doesn`t have binding at all
            var appBar = (ApplicationBar)Resources[_appBars[0]];
            ((ApplicationBarIconButton)appBar.Buttons[0]).Text = AppResources.add;
            ((ApplicationBarIconButton)appBar.Buttons[1]).Text = AppResources.refresh;

            ((ApplicationBarMenuItem)appBar.MenuItems[0]).Text = AppResources.setting;
            ((ApplicationBarMenuItem)appBar.MenuItems[1]).Text = AppResources.feedback;

            appBar = (ApplicationBar)Resources[_appBars[1]];
            ((ApplicationBarIconButton)appBar.Buttons[0]).Text = AppResources.check_all;
            ((ApplicationBarIconButton)appBar.Buttons[1]).Text = AppResources.refresh;

            ((ApplicationBarMenuItem)appBar.MenuItems[0]).Text = AppResources.setting;
            ((ApplicationBarMenuItem)appBar.MenuItems[1]).Text = AppResources.feedback;

            appBar = (ApplicationBar)Resources[_appBars[2]];
            ((ApplicationBarIconButton)appBar.Buttons[0]).Text = AppResources.check_all;
            ((ApplicationBarIconButton)appBar.Buttons[1]).Text = AppResources.refresh;

            ((ApplicationBarMenuItem)appBar.MenuItems[0]).Text = AppResources.setting;
            ((ApplicationBarMenuItem)appBar.MenuItems[1]).Text = AppResources.feedback;

            appBar = (ApplicationBar)Resources[_appBars[3]];
            ((ApplicationBarIconButton)appBar.Buttons[0]).Text = AppResources.remove_stars;
            ((ApplicationBarIconButton)appBar.Buttons[1]).Text = AppResources.refresh;

            ((ApplicationBarMenuItem)appBar.MenuItems[0]).Text = AppResources.setting;
            ((ApplicationBarMenuItem)appBar.MenuItems[1]).Text = AppResources.feedback;
        }

        private void AddChannelClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddChanel.xaml", UriKind.Relative));
        }

        private void ChanelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            App.ViewModel.Channel = (Channel)e.AddedItems[0];
            NavigationService.Navigate(new Uri("/ChannelPage.xaml", UriKind.Relative));

            ((ListBox)sender).SelectedItem = null;
        }

        private void ArticleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var element = (FrameworkElement)sender;

            App.ViewModel.Article = (Article)e.AddedItems[0];
            NavigationService.Navigate(new Uri(string.Format("/RssPage.xaml?val={0}", element.Tag), UriKind.Relative));

            ((ListBox)sender).SelectedItem = null;
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            _loadAction[Pivot.SelectedIndex]();
        }

        private void CheckClick(object sender, EventArgs e)
        {
            //Magic offset cos there is no tag prop
            _loadAction[Pivot.SelectedIndex + 10]();
        }

        private void PinChannelToStart(Channel channel)
        {
            string url = string.Format("/ChannelPage.xaml?ID={0}", channel.ID);
            ShellTile tileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(url));

            var secTileData = new StandardTileData
                {
                    Title = channel.Title,
                    Count = channel.NewCount,
                    BackgroundImage = new Uri("/Background.png", UriKind.RelativeOrAbsolute),
                    BackTitle = "Not read articles",
                    BackContent = channel.Title
                };


            // If the Tile was found, then update the background image.
            if (tileToFind != null)
            {
                tileToFind.Update(secTileData);
            }
            else
            {
                ShellTile.Create(new Uri(url, UriKind.RelativeOrAbsolute), secTileData);
            }
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            _loadAction[element.Tag]();
        }

        private void OnMenuClicked(object sender, RoutedEventArgs e)
        {
            var element = ((FrameworkElement)sender);
            _action[element.Tag]((Channel)element.DataContext);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.UpdateChannel((Channel)((FrameworkElement)sender).DataContext);
        }

        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var element = (Pivot)sender;
            ApplicationBar = (ApplicationBar)Resources[_appBars[element.SelectedIndex]];
        }

        private void NavigateToSetting(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Setting.xaml", UriKind.Relative));
        }

        private void LeaveFeedbackClick(object sender, EventArgs e)
        {
            App.LeaveFeedback();
        }
    }
}