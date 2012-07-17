using System.Data.Linq;

namespace WPRssReader.Model
{
    public class BaseDataContext : DataContext
    {
        // Pass the connection string to the base class.

        // Specify a table for the categories.
        public Table<Article> Articles;
        public Table<Channel> Channels;

        public BaseDataContext(string connectionString)
            : base(connectionString)
        {
        }
    }
}