using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsUtilities
{
    /// <summary>
    /// from http://steveclements.net/migrate-geekswithblogs-to-orchard-cms
    /// </summary>
    public class RssComments
    {
        IEnumerable<BlogMLComment> GetCommentsForPost(Post post)
        {
            string commentsUrl = _blogUrl + "/Comments/commentRss/" + post.postid.ToString() + ".aspx";

            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XDocument doc = XDocument.Load(commentsUrl);

            var comments = from d in doc.Descendants("item")
                           select new BlogMLComment
                           {
                               DateCreated = DateTime.Parse(d.Element("pubDate").Value),
                               UserName = d.Element(dc + "creator").Value,
                               Title = d.Element("title").Value,
                               Content = new BlogMLContent { Text = d.Element("description").Value }
                           };

            return comments ?? null;
        }
    }
}
