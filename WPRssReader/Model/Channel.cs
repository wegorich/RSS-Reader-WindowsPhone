using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace WPRssReader.Model
{
    [Table]
    public class Channel : NotifyProperty
    {
        // Define ID: private field, public property, and database column.
        private readonly EntitySet<Article> _articles;
        private int _id;
        private int _index;
        private DateTime? _lastUpdate;
        private int _newCount;
        private string _title;

        // Define item name: private field, public property, and database column.
        private string _url;
        [Column(IsVersion = true)] private Binary _version;

        public Channel()
        {
            _articles = new EntitySet<Article>(
                AttachArticle,
                DetachArticle
                );
            NewCount = Articles.Count;
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false,
            AutoSync = AutoSync.OnInsert)]
        public int ID
        {
            get { return _id; }
            set
            {
                if (_id == value) return;

                NotifyPropertyChanging("ID");
                _id = value;
                NotifyPropertyChanged("ID");
            }
        }

        [Column]
        public string URL
        {
            get { return _url; }
            set
            {
                if (_url == value) return;

                NotifyPropertyChanging("URL");
                _url = value;
                NotifyPropertyChanged("URL");
            }
        }

        // Define completion value: private field, public property, and database column.

        [Column]
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;

                NotifyPropertyChanging("Title");
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        // Define completion value: private field, public property, and database column.

        [Column]
        public DateTime? LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                if (_lastUpdate == value) return;

                NotifyPropertyChanging("LastUpdate");
                _lastUpdate = value;
                NotifyPropertyChanged("LastUpdate");
            }
        }

        // Version column aids update performance.

        [Association(Storage = "_articles", OtherKey = "ChannelID", ThisKey = "ID")]
        public EntitySet<Article> Articles
        {
            get { return _articles; }
            set { _articles.Assign(value); }
        }

        //All articles
        public IOrderedEnumerable<Article> AllArticles
        {
            get { return Articles.OrderByDescending(x => x.PubDate); }
        }

        [Column]
        public int Index
        {
            get { return _index; }
            set
            {
                if (value == _index) return;
                NotifyPropertyChanging("Index");
                _index = value;
                NotifyPropertyChanged("Index");
            }
        }

        public int NewCount
        {
            get { return _newCount; }
            set
            {
                if (value == _newCount) return;
                _newCount = value;
                NotifyPropertyChanged("NewCount");
            }
        }

        // Called during an add operation
        private void AttachArticle(Article article)
        {
            NotifyPropertyChanging("Article");
            article.Channel = this;

            NewCount++;
        }

        // Called during a remove operation
        private void DetachArticle(Article article)
        {
            NotifyPropertyChanging("Article");
            article.Channel = null;

            if (!article.IsRead)
                NewCount--;
        }
    }
}