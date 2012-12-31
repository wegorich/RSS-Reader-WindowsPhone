using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WPRssReader.Model;
using WPRssReader.Resources;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using System.Windows.Controls;
using MSPToolkit.Controls;
using System.Windows.Media;
using System.Threading;
using Coding4Fun.Phone.Controls;

namespace WPRssReader
{
    public partial class RssPage
    {
        private readonly Dictionary<object, Action> _action =
            new Dictionary<object, Action>();
        private readonly Pivot articlePivot = new Pivot ();
        private readonly Thickness _padding = new Thickness(12, 2, 12, 2);
        private readonly Thickness _bodyMargin = new Thickness(0, 6, 0, 100);
        private readonly Thickness _titleMargin = new Thickness(0, 0, 0, 6);

        private ObservableCollection<Article> _list;
        private string _value;
        private bool IsScrollingRight = true;
        private bool IsSelectedChanged = false;
        private int _itemsCount;

        public RssPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            //cos ApplicationBarIconButton doesn`t have binding at all
            var appBar = ApplicationBar;
            ((ApplicationBarMenuItem)appBar.MenuItems[0]).Text = AppResources.link;
            ((ApplicationBarMenuItem)appBar.MenuItems[1]).Text = AppResources.add_star;

            _action.Add(ApplicationBar.MenuItems[0], () => ShowArticleInBrowser(App.ViewModel.Article.Link));
            _action.Add(ApplicationBar.MenuItems[1], AddStar);

            _action.Add("all", () =>
                {
                    if (App.ViewModel.CanLoadAllArticles)
                    {
                        App.ViewModel.LoadNextAllArticles();
                    }
                });
            _action.Add("new", () =>
                {
                    if (App.ViewModel.CanLoadNewArticles)
                    {
                        App.ViewModel.LoadNextNewArticles();
                    }
                });
            _action.Add("stared", () =>
                {
                    if (App.ViewModel.CanLoadStaredArticles)
                    {
                        App.ViewModel.LoadNextStaredArticles();
                    }
                });
            _action.Add("channel", () => { });

            articlePivot.SelectionChanged+=PivotSelectionChanged;
            articlePivot.ManipulationCompleted+=PivotManipulationCompleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!NavigationContext.QueryString.ContainsKey("val")) return;

            _value = NavigationContext.QueryString["val"];

            switch (_value)
            {
                default:
                    _list = App.ViewModel.AllArticles;
                    break;
                case "new":
                    _list = App.ViewModel.NewArticles;
                    break;
                case "stared":
                    _list = App.ViewModel.StaredArticles;
                    break;
                case "channel":
                    _list = new ObservableCollection<Article>(App.ViewModel.Channel.AllArticles);
                    break;
            }
            if (articlePivot.Items.Count == 0)
            {
                var index = _list.IndexOf(App.ViewModel.Article);
                _itemsCount = _list.Count;
                articlePivot.Items.Add(new PivotItem());
                
                var art = AddArticleToView(App.ViewModel.Article, (PivotItem)articlePivot.Items[0]);
                if (_itemsCount > 1)
                {
                    articlePivot.Items.Add(new PivotItem());
                    AddArticleToView(_list[index + 1 != _itemsCount ? index + 1 : 0], (PivotItem)articlePivot.Items[1]);
                }
                if (_itemsCount > 2)
                {
                    articlePivot.Items.Add(new PivotItem());
                    AddArticleToView(_list[index == 0 ? _itemsCount - 1 : index - 1], (PivotItem)articlePivot.Items[2]);
                }
                this.Content=articlePivot;
            }
        }

        private void MenuItemVisibility()
        {
            if (App.ViewModel.Article == null) return;
            var articleIndex = _list.IndexOf(App.ViewModel.Article);
            _itemsCount = _list.Count;
            var isNextEnabled = articleIndex != _itemsCount - 1;
            var isPreviewEnabled = articleIndex != 0;

            if (IsScrollingRight && !isNextEnabled)
            {
                _action[_value]();
            }

            var article = IsScrollingRight ?
                        isNextEnabled ? _list.ElementAt(articleIndex + 1) : RepeatedArticle() :
                        isPreviewEnabled ? _list.ElementAt(articleIndex - 1) : RepeatedArticle(false);

            if (article != null && _itemsCount != articlePivot.Items.Count)
            {
                AddArticleToView(article, (PivotItem)articlePivot.Items[IsScrollingRight ?
                                                                              (articlePivot.SelectedIndex + 1) % 3 :
                                                                              articlePivot.SelectedIndex == 0 ? 2 :
                                                                                                                   articlePivot.SelectedIndex - 1]);
            }
        }

        public Article RepeatedArticle(bool isLast = true)
        {
            if (_itemsCount > 3)
            {
                ToastPrompt toast = new ToastPrompt
                {
                    MillisecondsUntilHidden = 1500,
                    Foreground = App.WhiteColor,
                    Message = !isLast ? AppResources.feed_item_first : AppResources.feed_item_last
                };

                toast.Show();
            }

            return _list[isLast ? 0 : _itemsCount - 1];
        }

        #region Bar buttons

        private void BarButtonClick(object sender, EventArgs e)
        {
            _action[sender]();
        }

        private void ShowArticleInBrowser(string url)
        {
            if (App.ViewModel.Article == null) return;

            var web = new WebBrowserTask
                {
                    Uri = url != null
                            ? new Uri(url, UriKind.Absolute)
                            : null
                };
            web.Show();
        }

        private void AddStar()
        {
            App.ViewModel.Article.IsStared = !App.ViewModel.Article.IsStared;

            ToastPrompt toast = new ToastPrompt
            {
                MillisecondsUntilHidden = 1500,
                Foreground = App.WhiteColor,
                Message = App.ViewModel.Article.IsStared ? AppResources.star_added : AppResources.star_removed
            };

            toast.Show();
        }

        #endregion

        private object AddArticleToView(Article art, PivotItem item)
        {
            art.IsRead = true;

            var scroll = new ScrollViewer();
            var panel = new StackPanel();
            scroll.Content = panel;
            panel.Children.Add(new HTMLTextBox()
            {
                Html = String.Format("<a style='color:{2}' href='{0}'>{1}</a>", art.Link, art.Title,App.ViewModel.Foreground),
                Margin = _titleMargin,
                FontSize = 28
            });

            panel.Children.Add(new Border
            {
                Background = (Brush)App.Current.Resources["PhoneAccentBrush"],
                Child = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = RssViewModel.DateConvertor.ToUserFriendlyString(art.PubDate).ToString(),
                    FontSize = 20,
                    Margin = _padding,
                    Foreground = App.WhiteColor
                },
                Margin = _padding
            });

            panel.Children.Add(new HTMLTextBox
            {
                Html = "<style type='text/css'>a{color:gray;}</style>" + art.Description,
                Padding = _bodyMargin,
                FontSize = 24
            });

            item.Content = scroll;
            item.Tag = art;
            return panel;
        }

        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removeIndex = articlePivot.Items.IndexOf(e.RemovedItems[0]);
            var addIndex = articlePivot.Items.IndexOf(e.AddedItems[0]);

            App.ViewModel.Article = (Article)((PivotItem)e.AddedItems[0]).Tag;

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
        private void OnApplicationBarStateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            var appBar = sender as ApplicationBar;
            if (appBar == null) return;

            appBar.Opacity = e.IsMenuVisible ? 1 : .65;
        }
    }
}