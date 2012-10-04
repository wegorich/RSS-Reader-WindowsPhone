using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using WPRssReader.Model;

namespace WPRssReader.Helper
{
    public class ParseRss
    {
        private readonly RssViewModel _model;

        public ParseRss(RssViewModel model)
        {
            _model = model;
        }

        public void AddChannel(Channel c)
        {
            GetArticles(c);
        }

        public void GetArticles(Channel c)
        {
            if (c.LastUpdate.HasValue)
            {
                DateTime last = c.LastUpdate.Value;
                DateTime now = DateTime.Now;
                if (now.Minute == last.Minute && (now - last).TotalMinutes < 5) return;
            }

            c.LastUpdate = Convert.ToDateTime("1900/01/01");
            var client = new WebClient();

            client.OpenReadCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        c.LastUpdate = Convert.ToDateTime("1901/01/01");
                        return;
                    }

                    var str = e.Result;
                    try
                    {
                        Read(str, c, _model);
                        _model.SubmitChanges();
                        _model.RefreshArticles();
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    finally
                    {
                        str.Close();
                    }
                };
            try
            {
                client.OpenReadAsync(new Uri(c.URL, UriKind.Absolute));
            }
            catch
            {
                App.ViewModel.DeleteChannel(c);
            }
        }

        private void Read(Stream stream, Channel c, RssViewModel m)
        {
            var feedFormatter = new Atom10FeedFormatter();
            var rssFormater = new Rss20FeedFormatter();

            XmlReader atomReader = XmlReader.Create(stream);
            SyndicationFeedFormatter f = null;

            if (feedFormatter.CanRead(atomReader))
            {
                feedFormatter.ReadFrom(atomReader);
                atomReader.Close();
                f = feedFormatter;
            }
            else
                if (rssFormater.CanRead(atomReader))
                {
                    rssFormater.ReadFrom(atomReader);
                    atomReader.Close();
                    f = rssFormater;
                }

            if (f == null) return;

            SyndicationFeed feed = f.Feed;
            c.Title = feed.Title.Text;

            Article[] articles = feed.Items.Select(item => new Article
                {
                    PubDate = item.PublishDate.DateTime,
                    Description = item.Summary.Text,
                    Link = item.Links[0].Uri.OriginalString,
                    Title = item.Title.Text,
                }).Select(art => m.AddArticle(art, c)).ToArray();
            m.DeleteArticle(c.Articles.Where(x => !x.IsStared).Except(articles).ToArray(), c);
            c.LastUpdate = DateTime.Now;
        }
    }
}