using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using WPRssReader.Model;
using WPRssReader.Resources;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls;
using Coding4Fun.Phone.Controls;

namespace WPRssReader
{
    public partial class ChannelPage
    {
        private readonly ApplicationBarIconButton _next;
        private readonly ApplicationBarIconButton _preview;
        private int _listCount;
        private ObservableCollection<Channel> _list;
        private ObservableCollection<Channel> _navList;
        private bool IsScrollingRight = true;
        private bool IsSelectedChanged = false;

        public ChannelPage()
        {
            InitializeComponent();
            AddNavChannel();
            DataContext = App.ViewModel;

            //cos ApplicationBarIconButton doesn`t have binding at all
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.check_all;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = AppResources.refresh;
        }

        private void AddNavChannel()
        {
            App.ViewModel.ChannelsNavList =
                _navList = new ObservableCollection<Channel>();
            _list = App.ViewModel.Channels;
            
            if (App.ViewModel.Channel == null) return;
            _navList.Add(App.ViewModel.Channel);
            
            var index = _list.IndexOf(App.ViewModel.Channel);
            _listCount = _list.Count;
            if (_listCount > 1)
            {
                _navList.Add(_list[index + 1 != _listCount ? index + 1 : 0]);
            }
            if (_listCount > 2)
            {
                _navList.Add(_list[index == 0 ? _listCount - 1 : index - 1]);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (!NavigationContext.QueryString.ContainsKey("ID")) return;

                int id;
                int.TryParse(NavigationContext.QueryString["ID"], out id);

                App.ViewModel.Channel = App.ViewModel.Channels.FirstOrDefault(x => x.ID == id);
                
                if (App.ViewModel.Channel == null)
                {
                    //try fix wrong user case
                    App.ViewModel.Channel = App.ViewModel.Channels.FirstOrDefault();
                }
                if (App.ViewModel.Channel != null)
                {
                    App.ViewModel.UpdateChannel(App.ViewModel.Channel);
                }
                else
                {
                    //If user have got pinned tile but removed all data in the app
                    this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                AddNavChannel();
            }
        }

        private void ArticleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            App.ViewModel.Article = (Article)e.AddedItems[0];
            NavigationService.Navigate(new Uri("/RssPage.xaml?val=channel", UriKind.Relative));
            ((ListBox)sender).SelectedItem = null;
        }

        private void CheckAllClick(object sender, EventArgs e)
        {
            foreach (Article art in App.ViewModel.Channel.Articles.Where(x => x.IsRead == false))
            {
                art.IsRead = true;
            }
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            App.ViewModel.RefreshChannel(App.ViewModel.Channel);
        }

        private void MenuItemVisibility()
        {
            if (App.ViewModel.Channel == null) return;

            var index = _list.IndexOf(App.ViewModel.Channel);
            _listCount = _list.Count;

            var isNextEnabled = index != _listCount - 1;
            var isPreviewEnabled = index != 0;

            var channel = IsScrollingRight ?
                        isNextEnabled ? _list.ElementAt(index + 1) : RepeatedArticle() :
                        isPreviewEnabled ? _list.ElementAt(index - 1) : RepeatedArticle(false);

            if (channel != null && _listCount != pivotChannel.Items.Count)
            {
                _navList[IsScrollingRight ? (pivotChannel.SelectedIndex + 1) % 3 : 
                    pivotChannel.SelectedIndex == 0 ? 2 : 
                    pivotChannel.SelectedIndex - 1] = channel;
            }
        }

        public Channel RepeatedArticle(bool isLast = true)
        {
            if (_listCount > 3)
            {
                ToastPrompt toast = new ToastPrompt
                {
                    MillisecondsUntilHidden = 1500,
                    Foreground = App.WhiteColor,
                    Message = !isLast ? AppResources.feed_item_first : AppResources.feed_item_last
                };

                toast.Show();
            }

            return _list[isLast ? 0 : _listCount - 1];
        }


        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removeIndex = pivotChannel.Items.IndexOf(e.RemovedItems[0]);
            var addIndex = pivotChannel.Items.IndexOf(e.AddedItems[0]);

            App.ViewModel.Channel = (Channel)(e.AddedItems[0]);

            IsScrollingRight = removeIndex == 2 ? addIndex == 0 : removeIndex + 1 == addIndex;
            IsSelectedChanged = true;
        }

        private void PivotManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (IsSelectedChanged)
            {
                MenuItemVisibility();
                IsSelectedChanged = false;
            }
        }
    }
}