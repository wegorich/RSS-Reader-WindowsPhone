using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
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

                    if (e.Result == null) return;

                    try
                    {
                        Read(e.Result, c, _model);
                        _model.SubmitChanges();
                        _model.RefreshArticles();
                    }
                    catch (Exception ex)
                    {
                        return;
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

        private void Read(Stream content, Channel c, RssViewModel m)
        {
            var ecoding = new MSPToolkit.Encodings.Windows1251Encoding();
            var xml = new StreamReader(content, ecoding).ReadToEnd();
            //if (xml.Contains("encoding=\"windows-1251\""))
            //{
            //xml = new StreamReader(content, ecoding).ReadToEnd();
            xml = xml.Replace("encoding=\"windows-1251\"", "encoding=\"utf-8\"");
                
            //}
            

            xml = xml.Replace("<lastBuildDate></lastBuildDate>", "");
            var b = Encoding.UTF8.GetBytes(xml);

            var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, IgnoreWhitespace = true, IgnoreComments = true};
            var reader = XmlReader.Create(new MemoryStream(b), settings);
            var feed = SyndicationFeed.Load(reader);

            if (feed == null) return;
            c.Title = feed.Title.Text;

            var articles = feed.Items.Select(item => new Article
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