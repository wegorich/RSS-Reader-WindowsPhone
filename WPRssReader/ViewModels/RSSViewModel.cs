using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using Apex.MVVM;
using WPRssReader.Helper;
using WPRssReader.Model;

namespace WPRssReader
{
    public class RssViewModel : BaseNotifyPropertyChanged
    {
        public static int SelectCount = 25;
        private readonly Command _addChannelCommand;

        private readonly ParseRss _parseRss;
        private readonly Command _refreshCommand;
        private readonly BaseDataContext _rssDb;

        public string Accent;
        public string Background;
        public string Foreground;

        private ObservableCollection<Article> _allArticles;
        private Article _article;

        // LINQ to SQL data context for the local database.

        //Selected channel
        private Channel _channel;
        private ObservableCollection<Channel> _channels;
        private ObservableCollection<Article> _newArticles;
        private ObservableCollection<Article> _staredArticles;


        public RssViewModel(string rssDbConnectionString)
        {
            _parseRss = new ParseRss(this);
            _rssDb = new BaseDataContext(rssDbConnectionString);

            _addChannelCommand = new Command(DoAddChanel);

            AllArticles = new ObservableCollection<Article>();
            NewArticles = new ObservableCollection<Article>();
            StaredArticles = new ObservableCollection<Article>();
        }

        public Channel Channel
        {
            get { return _channel; }
            set
            {
                if (_channel == value) return;
                _channel = value;
                NotifyPropertyChanged("Channel");
            }
        }

        //Selected article
        public Article Article
        {
            get { return _article; }
            set
            {
                if (_article == value) return;
                _article = value;
                NotifyPropertyChanged("Article");
            }
        }

        public ObservableCollection<Channel> Channels
        {
            get { return _channels; }
            set
            {
                _channels = value;
                NotifyPropertyChanged("Channels");
            }
        }

        //All articles
        public int NewCount
        {
            get { return _rssDb.Articles.Where(x => !x.IsRead).Count(); }
        }

        public ObservableCollection<Article> AllArticles
        {
            get { return _allArticles; }
            set
            {
                _allArticles = value;
                NotifyPropertyChanged("AllArticles");
            }
        }

        //Not read articles

        public ObservableCollection<Article> NewArticles
        {
            get { return _newArticles; }
            set
            {
                _newArticles = value;
                NotifyPropertyChanged("NewArticles");
            }
        }

        public ObservableCollection<Article> StaredArticles
        {
            get { return _staredArticles; }
            set
            {
                _staredArticles = value;
                NotifyPropertyChanged("StaredArticles");
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public Command AddChannelCommand
        {
            get { return _addChannelCommand; }
        }

        #region Can Load / Load - functions

        public bool CanLoadAllArticles
        {
            get { return AllArticles.Count < _rssDb.Articles.Count(); }
        }

        public bool CanLoadNewArticles
        {
            get { return NewArticles.Count < _rssDb.Articles.Where(x => !x.IsRead).Count(); }
        }

        public bool CanLoadStaredArticles
        {
            get { return StaredArticles.Count < _rssDb.Articles.Where(x => x.IsStared).Count(); }
        }

        public void LoadNextAllArticles()
        {
            IOrderedQueryable<Article> articlesInDb = _rssDb.Articles.OrderByDescending(x => x.PubDate);
            foreach (Article a in articlesInDb.Skip(AllArticles.Count).Take(SelectCount))
            {
                AllArticles.Add(a);
            }
            NotifyPropertyChanged("CanLoadAllArticles");
        }

        public void LoadNextNewArticles()
        {
            IOrderedQueryable<Article> articlesInDb =
                _rssDb.Articles.Where(x => !x.IsRead).OrderByDescending(x => x.PubDate);
            foreach (Article a in articlesInDb.Skip(NewArticles.Count).Take(SelectCount))
            {
                NewArticles.Add(a);
            }
            NotifyPropertyChanged("CanLoadNewArticles");
        }

        public void LoadNextStaredArticles()
        {
            IOrderedQueryable<Article> articlesInDb =
                _rssDb.Articles.Where(x => x.IsStared).OrderByDescending(x => x.StaredDate);
            foreach (Article a in articlesInDb.Skip(StaredArticles.Count).Take(SelectCount))
            {
                StaredArticles.Add(a);
            }
            NotifyPropertyChanged("CanLoadStaredArticles");
        }

        #endregion

        // Write changes in the data context to the database.
        public void SaveChangesToDb()
        {
            _rssDb.SubmitChanges();
        }

        // Query database and load the collections and list used by the pivot pages.
        public void LoadCollectionsFromDatabase()
        {
            RefreshArticles();

            // Query the database and load all to-do items.
            Channels = new ObservableCollection<Channel>(_rssDb.Channels.OrderBy(x => x.Index));
            foreach (Channel c in Channels)
            {
                c.NewCount = c.Articles.Where(x => !x.IsRead).Count();
            }
        }

        #region Add / Delete / Submit changes

        // Add a to-do item to the database and collections.
        public Article AddArticle(Article article, Channel channel)
        {
            Article art = channel.Articles.FirstOrDefault(x => x.Link == article.Link);
            if (art == null)
            {
                channel.Articles.Add(article);
                // Add a to-do item to the data context.
                _rssDb.Articles.InsertOnSubmit(article);
                return article;
            }
            return art;
        }

        // Remove a to-do task item from the database and collections.
        public void DeleteArticle(Article[] articles, Channel channel)
        {
            foreach (Article art in articles)
            {
                DeleteArticle(art, channel);
            }
        }

        // Remove a to-do task item from the database and collections.
        public void DeleteArticle(Article article, Channel channel)
        {
            // Remove the to-do item from the "all" observable collection.
            AllArticles.Remove(article);
            NewArticles.Remove(article);

            channel.Articles.Remove(article);
            // Remove the to-do item from the data context.
            _rssDb.Articles.DeleteOnSubmit(article);
        }

        // Add a to-do item to the database and collections.
        public bool AddChannel(Channel channel)
        {
            if (!_rssDb.Channels.Any(o => o.URL == channel.URL))
            {
                // Add a to-do item to the data context.
                _rssDb.Channels.InsertOnSubmit(channel);

                // Add a to-do item to the "all" observable collection.
                channel.Index = Channels.Count;
                Channels.Add(channel);

                return true;
            }
            return false;
        }

        // Remove a to-do task item from the database and collections.
        public void DeleteChannel(Channel channel)
        {
            // Remove the to-do item from the "all" observable collection.
            Channels.Remove(channel);

            foreach (Article article in channel.Articles)
            {
                AllArticles.Remove(article);
                if (!article.IsRead)
                {
                    NewArticles.Remove(article);
                }
                _rssDb.Articles.DeleteOnSubmit(article);
            }

            // Remove the to-do item from the data context.
            _rssDb.Channels.DeleteOnSubmit(channel);
        }

        public void SubmitChanges()
        {
            // Save changes to the database.
            _rssDb.SubmitChanges();
        }

        #endregion

        #region Functions

        #region Refresh

        public void RefreshAll()
        {
            SaveChangesToDb();
            AllArticles.Clear();
            LoadNextAllArticles();
        }

        public void RefreshNew()
        {
            SaveChangesToDb();
            NewArticles.Clear();
            LoadNextNewArticles();
        }

        public void RefreshStared()
        {
            SaveChangesToDb();
            StaredArticles.Clear();
            LoadNextStaredArticles();
        }

        public void RefreshArticles()
        {
            RefreshAll();
            RefreshNew();
            RefreshStared();
        }

        #endregion

        #region Read all

        public void ReadAll()
        {
            ReadArticles(AllArticles);
        }

        public void ReadNew()
        {
            ReadArticles(NewArticles);
        }

        private void ReadArticles(ObservableCollection<Article> articles)
        {
            foreach (Article art in articles.Where(x => x.IsRead == false))
            {
                art.IsRead = true;
            }
        }

        #endregion

        /// <summary>
        /// The Command function.
        /// </summary>

        public void MoveChannelUp(Channel ch)
        {
            int index = Channels.IndexOf(ch) - 1;
            if (index < 0) return;

            Channels.Remove(ch);
            Channels.Insert(index, ch);
            ch.Index = index;
        }

        public void MoveChannelDown(Channel ch)
        {
            int index = Channels.IndexOf(ch) + 1;
            if (index == Channels.Count) return;

            Channels.Remove(ch);
            Channels.Insert(index, ch);
            ch.Index = index;
        }

        private void DoAddChanel(object data)
        {
            var rss = data as string;
            if (String.IsNullOrEmpty(rss)) return;
            var c = new Channel {URL = rss, Title = rss};
            c = AddChannel(c) ? c : Channels.First(x => x.URL == c.URL);
            _parseRss.AddChannel(c);
        }

        public void AddStar(Article art)
        {
            art.IsStared = true;
            StaredArticles.Insert(0, art);
        }

        public void RemoveStar(Article art)
        {
            art.IsStared = false;
            StaredArticles.Remove(art);
        }

        public void UpdateChannel(Channel ch)
        {
            _parseRss.GetArticles(ch);
        }

        public void DoAddNewArticles()
        {
            foreach (Channel ch in Channels)
            {
                _parseRss.GetArticles(ch);
            }
            RefreshArticles();
        }

        public static string ConvertExtendedAscii(string html)
        {
            string retVal = "";
            char[] s = html.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    retVal += "&#" + Convert.ToInt32(c) + ";";
                else
                    retVal += c;
            }

            return retVal;
        }

        public bool BuildHTML(string description, string fileName)
        {
            var b = new StringBuilder();
            b.Append("<script src='jquery.js' type='text/javascript'></script>" +
                     "<script src='jquery.lazyload.js' type='text/javascript'></script>" +
                     "<script type='text/javascript'>" +
                     "$(function(){" +
                     "$('img').lazyload().css('width', '100%');" +
                     "});</script>");
            b.Append("<style type='text/css'>" +
                     "*{" +
                     "background:");
            b.Append(Background);
            b.Append(";color:");
            b.Append(Foreground);
            b.Append(";}" +
                     "a {" +
                     "color:");
            b.Append(Accent);
            b.Append("}" +
                     "</style>");
            b.Append(description.Replace(" src=", " src='gray.jpg' data-original="));

            byte[] bytes = Encoding.UTF8.GetBytes(ConvertExtendedAscii(b.ToString()));
            // Get the iso store
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    // Write the html in the html/index.html
                    using (IsolatedStorageFileStream output = iso.CreateFile(fileName))
                    {
                        output.Write(bytes, 0, bytes.Length);
                        output.Close();
                    }
                }
                catch (Exception)
                {
                    iso.Dispose();
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}