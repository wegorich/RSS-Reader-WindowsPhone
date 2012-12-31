using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace WPRssReader.Model
{
    [Table]
    public class Article : NotifyProperty
    {
        // Define ID: private field, public property, and database column.
        [Column] internal int ChannelID;

        // Entity reference, to identify the ToDoCategory "storage" table
        private EntityRef<Channel> _channel;
        private string _description;
        private int _id;
        private bool _isRead;
        private bool _isStared;
        private string _link;
        private DateTime? _pubDate;
        private DateTime? _addDate;
        private DateTime? _staredDate;

        // Define category name: private field, public property, and database column.
        private string _title;
        [Column(IsVersion = true)] private Binary _version;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false,
            AutoSync = AutoSync.OnInsert)]
        public int ID
        {
            get { return _id; }
            set
            {
                NotifyPropertyChanging("ID");
                _id = value;
                NotifyPropertyChanged("ID");
            }
        }

        [Column]
        public string Title
        {
            get { return _title; }
            set
            {
                NotifyPropertyChanging("Title");
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        // Define category name: private field, public property, and database column.

        [Column]
        public string Link
        {
            get { return _link; }
            set
            {
                NotifyPropertyChanging("Link");
                _link = value;
                NotifyPropertyChanged("Link");
            }
        }

        // Define category name: private field, public property, and database column.

        [Column]
        public string Description
        {
            get { return _description; }
            set
            {
                NotifyPropertyChanging("Link");
                _description = value;
                NotifyPropertyChanged("Link");
            }
        }

        // Define completion value: private field, public property, and database column.

        [Column]
        public DateTime? PubDate
        {
            get { return _pubDate; }
            set
            {
                if (_pubDate != value)
                {
                    NotifyPropertyChanging("PubDate");
                    _pubDate = value;
                    NotifyPropertyChanged("PubDate");
                }
            }
        }

        #if DB_VERSION_1
        [Column]
        public DateTime? AddDate
        {
            get { return _addDate; }
            set
            {
                if (_addDate == value) return;

                NotifyPropertyChanging("AddDate");
                _addDate = value;
                NotifyPropertyChanged("AddDate");
            }
        }
        #endif

        // Define completion value: private field, public property, and database column.

        [Column]
        public DateTime? StaredDate
        {
            get { return _staredDate; }
            set
            {
                if (_staredDate != value)
                {
                    NotifyPropertyChanging("ReadDate");
                    _staredDate = value;
                    NotifyPropertyChanged("ReadDate");
                }
            }
        }

        // Define completion value: private field, public property, and database column.

        [Column]
        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                if (_isRead != value)
                {
                    NotifyPropertyChanging("IsRead");
                    if (value && Channel != null)
                    {
                        Channel.NewCount--;
                    }
                    _isRead = value;
                    NotifyPropertyChanged("IsRead");
                }
            }
        }

        // Define completion value: private field, public property, and database column.

        [Column]
        public bool IsStared
        {
            get { return _isStared; }
            set
            {
                if (_isStared != value)
                {
                    NotifyPropertyChanging("IsStared");
                    StaredDate = value ? (DateTime?) DateTime.Now : null;
                    _isStared = value;
                    NotifyPropertyChanged("IsStared");
                }
            }
        }

        // Version column aids update performance.

        // Association, to describe the relationship between this key and that "storage" table
        [Association(Storage = "_channel", ThisKey = "ChannelID", OtherKey = "ID", IsForeignKey = true)]
        public Channel Channel
        {
            get { return _channel.Entity; }
            set
            {
                NotifyPropertyChanging("Channel");
                _channel.Entity = value;

                if (value != null)
                {
                    ChannelID = value.ID;
                }

                NotifyPropertyChanging("Channel");
            }
        }
    }
}