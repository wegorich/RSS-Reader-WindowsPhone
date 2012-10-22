using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using WPRssReader.Model;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Coding4Fun.Phone.Controls;
using WPRssReader.Resources;

namespace WPRssReader.Helper
{
    public class ParseRss
    {
        private const string EncodedUTF8 = "encoding=\"utf-8\"";
        private const int LengthOfEnc = 10;// "encoding=\"".Length;
        private const string RexForEncoding = "encoding=\".*\"";
        private const string RemoveLastBuildDate = "<lastBuildDate></lastBuildDate>";
        private const string MinDBTime = "1900/01/01";
        private const string MinDBUpdated = "1901/01/01";

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

            c.LastUpdate = Convert.ToDateTime(MinDBTime);
            var client = new WebClient();

            client.OpenReadCompleted += (sender, e) =>
                {
                    if (e.Error != null||e.Result == null)
                    {
                        c.LastUpdate = Convert.ToDateTime(MinDBUpdated);
                        return;
                    }

                    try
                    {
                        Read(GetEncodedString(e.Result), c, _model);
                        _model.SubmitChanges();
                        _model.RefreshArticles();
                    }
                    catch (Exception ex)
                    {
                        ToastPrompt toast = new ToastPrompt();
                        toast.MillisecondsUntilHidden = 1500;
                        toast.Message = AppResources.add_message_error;
                        toast.Show();
                        c.LastUpdate = Convert.ToDateTime(MinDBUpdated);
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
                var toast = new ToastPrompt{
                                             MillisecondsUntilHidden = 1500,
                                             Message = AppResources.channel_removed
                                           };
                toast.Show();
            }
        }

        private void Read(string xml, Channel c, RssViewModel m)
        {
            xml = xml.Replace(RemoveLastBuildDate, String.Empty);

            var b = Encoding.UTF8.GetBytes(xml);

            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            var feed = SyndicationFeed.Load(XmlReader.Create(new MemoryStream(b), settings));

            if (feed == null) return;
            c.Title = feed.Title.Text;

            var articles = feed.Items.Select(item => new Article
                {
                    PubDate = item.PublishDate.DateTime,
                    //The reason for the 8,060-byte limit 
                    Description = item.Summary.Text.Length < 1000 ? item.Summary.Text : item.Summary.Text.Substring(0, 1000),
                    Link = item.Links[0].Uri.OriginalString,
                    Title = item.Title.Text,
                }).Select(art => m.AddArticle(art, c)).ToArray();

            m.DeleteArticle(c.Articles.Where(x => !x.IsStared).Except(articles).ToArray(), c);
            c.LastUpdate = DateTime.Now;
        }

        private string GetEncodedString(Stream content)
        {
            var bytes = new byte[content.Length];
            content.Read(bytes, 0, bytes.Length);
            var xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var match = Regex.Match(xml, RexForEncoding, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var enc = match.Value;
                var encoding = MSPToolkit.Encodings.BaseSingleByteEncoding.GetEncoding(enc.Substring(LengthOfEnc, enc.Length - LengthOfEnc - 1));//-1 last "
                if (encoding != null)
                {
                    xml = encoding.GetString(bytes, 0, bytes.Length);
                }
                xml = xml.Replace(enc, EncodedUTF8);
            }
            return xml;
        }
    }
}