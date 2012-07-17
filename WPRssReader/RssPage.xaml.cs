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

namespace WPRssReader
{
    public partial class RssPage
    {
        private readonly Dictionary<object, Action> _action =
            new Dictionary<object, Action>();

        private readonly ApplicationBarIconButton _next;

        private readonly ApplicationBarIconButton _preview;
        private readonly ApplicationBarIconButton _stared;
        private ObservableCollection<Article> _list;
        private string _value;

        public RssPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            _preview = (ApplicationBarIconButton) ApplicationBar.Buttons[0];
            _stared = (ApplicationBarIconButton) ApplicationBar.Buttons[2];
            _next = (ApplicationBarIconButton) ApplicationBar.Buttons[3];

            _action.Add(ApplicationBar.Buttons[0], PreviewArticle);
            _action.Add(ApplicationBar.Buttons[1], ShowArticleInBrowser);
            _action.Add(ApplicationBar.Buttons[2], AddOrRemoveStar);
            _action.Add(ApplicationBar.Buttons[3], NextArticle);

            _action.Add("all", () =>
                                   {
                                       if (App.ViewModel.CanLoadAllArticles)
                                       {
                                           App.ViewModel.LoadNextAllArticles();
                                           _next.IsEnabled = true;
                                       }
                                   });
            _action.Add("new", () =>
                                   {
                                       if (App.ViewModel.CanLoadNewArticles)
                                       {
                                           App.ViewModel.LoadNextNewArticles();
                                           _next.IsEnabled = true;
                                       }
                                   });
            _action.Add("stared", () =>
                                      {
                                          if (App.ViewModel.CanLoadStaredArticles)
                                          {
                                              App.ViewModel.LoadNextStaredArticles();
                                              _next.IsEnabled = true;
                                          }
                                      });
            _action.Add("channel", () => { });

            //cos ApplicationBarIconButton doesn`t have binding at all
            ((ApplicationBarIconButton) ApplicationBar.Buttons[0]).Text = AppResources.preview;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).Text = AppResources.link;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[2]).Text = AppResources.add_star;
            ((ApplicationBarIconButton) ApplicationBar.Buttons[3]).Text = AppResources.next;
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

            MenuItemVisibility();
        }

        private void WebBrowserNavigated(object sender, NavigationEventArgs e)
        {
            var element = (WebBrowser) sender;
            element.Visibility = Visibility.Visible;
        }

        private void ArticleDoubleTap(object sender, GestureEventArgs e)
        {
            ShowArticleInBrowser();
        }

        private void MenuItemVisibility()
        {
            if (App.ViewModel.Article != null)
            {
                App.ViewModel.Article.IsRead = true;

                _stared.IconUri =
                    new Uri(
                        String.Format("/Toolkit.Content/favs.{0}.png", App.ViewModel.Article.IsStared ? "remove" : "add"),
                        UriKind.Relative);
                _next.IsEnabled = _list.IndexOf(App.ViewModel.Article) != _list.Count - 1;
                if (_next.IsEnabled == false)
                {
                    _action[_value]();
                }
                _preview.IsEnabled = _list.IndexOf(App.ViewModel.Article) != 0;

                BrowserNavigate();
            }
            else
            {
                _next.IsEnabled =
                    _preview.IsEnabled = false;
            }
        }

        private void BrowserNavigate()
        {
            const string fileName = "index.html";
            if (App.ViewModel.BuildHTML(App.ViewModel.Article.Description, fileName))
            {
                Browser.Visibility = Visibility.Collapsed;
                Browser.Navigate(new Uri(fileName, UriKind.Relative));
            }
        }

        #region Bar buttons

        private void BarButtonClick(object sender, EventArgs e)
        {
            _action[sender]();
        }

        private void NextArticle()
        {
            int number = _list.IndexOf(App.ViewModel.Article) + 1;

            App.ViewModel.Article = _list.ElementAt(number);
            MenuItemVisibility();
        }

        private void ShowArticleInBrowser()
        {
            if (App.ViewModel.Article == null) return;

            var web = new WebBrowserTask
                          {
                              Uri =
                                  App.ViewModel.Article.Link != null
                                      ? new Uri(App.ViewModel.Article.Link, UriKind.Absolute)
                                      : null
                          };
            web.Show();
        }

        private void PreviewArticle()
        {
            int number = _list.IndexOf(App.ViewModel.Article) - 1;

            App.ViewModel.Article = _list.ElementAt(number);
            MenuItemVisibility();
        }

        private void AddOrRemoveStar()
        {
            App.ViewModel.Article.IsStared = ! App.ViewModel.Article.IsStared;
            const string str = "/Toolkit.Content/favs.{0}.png";
            string str2;
            if (App.ViewModel.Article.IsStared)
            {
                str2 = "remove";
                _stared.Text = AppResources.remove_star;
            }
            else
            {
                str2 = "add";
                _stared.Text = AppResources.add_star;
            }
            _stared.IconUri = new Uri(String.Format(str, str2), UriKind.Relative);
        }

        #endregion
    }
}