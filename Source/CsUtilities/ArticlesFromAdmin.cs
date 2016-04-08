using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using BlogML;
using BlogML.Xml;
using Common;
using HtmlAgilityPack;

namespace CsUtilities
{
    public class ArticlesFromAdmin
    {
        /// <summary>
        /// In a loop call http://www.geekswithblogs.net/mnf/Admin/EditArticles.aspx?pg=0 and save each article

        /// </summary>
        /// <param name="blogUrl"></param>
        /// <param name="attemptsCount"></param>

        public static List<BlogMLPost> GetAllArticleLinks(string blogUrl, SiteAuthentication authentication, List<ArticleCategory> categories, string dirSavePath)
        {
             int attemptsPage = 100;
         
            var articleLinks = new HashSet<string>();
            for (int i = 0; i < attemptsPage; i++)
            {
                string content = OpenEditArticlesPage(blogUrl, i, authentication,dirSavePath);
                int countBefore = articleLinks.Count;
                articleLinks=  AddArticleLinksFromPage(content, articleLinks);
                if (countBefore == articleLinks.Count) break;
            }

            var posts = new List<BlogMLPost>();
            foreach (var articleUrl in articleLinks)
            {
                //e.g. http://geekswithblogs.net/mnf/archive/2007/09/12/my-datetimehelper-class.aspx
                string content = OpenPage(articleUrl);
                if (!String.IsNullOrEmpty(content))
                {
                    string path = dirSavePath + @"\Articles\" + articleUrl.RightAfterLast("/")+".html";
                    EnsureDirectoryExists(path);
                    File.WriteAllText(path, content);
                }
                   var post = ArticleToBlogMlPost(categories, content, articleUrl);
                posts.Add(post);
            }
            return posts;
        }

    

        private static string OpenEditArticlesPage(string blogUrl, int i,SiteAuthentication authentication, string dirSavePath)
        {
            string page = blogUrl + String.Format("/Admin/EditArticles.aspx?pg={0}", i);
            if (authentication.CookieContainer == null)
            {
                authentication.CookieContainer = OpenLoginPage(blogUrl, authentication);
            }
            string content = OpenPage(page, authentication.CookieContainer);
            if (!String.IsNullOrEmpty(content))
            {
                //http://stackoverflow.com/questions/9907682/create-a-txt-file-if-doesnt-exist-and-if-it-does-append-a-new-line
                string path = dirSavePath + String.Format("ArticlesPage{0}.htm", i);
                EnsureDirectoryExists(path);
                File.WriteAllText(path, content);
               
            }
            return content;
        }

    

        private static HashSet<string> AddArticleLinksFromPage(string content, HashSet<string> articleLinks)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            //<a title="View Entry" href="http://geekswithblogs.net/mnf/archive/2007/07/17/My-CollectionsHelper-class.aspx">My CollectionsHelper class</a>
            var linksToArticles = doc.DocumentNode.SelectNodes("//a[@title='View Entry']");
            if (linksToArticles != null)
            {
                foreach (var link in linksToArticles)
                {
                    string url = link.Attributes["href"].Value;
                    if (!url.IsNullOrEmpty())
                    {
                        articleLinks.Add(url);
                    }
                }
            }
            return articleLinks;
        }

        public static List<ArticleCategory> LoadCategories(string blogUrl, SiteAuthentication authentication, string dirSavePath)
        {
            string content = OpenEditArticlesPage(blogUrl, 0, authentication, dirSavePath);
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            //<li><a href="http://www.geekswithblogs.net/mnf/Admin/EditArticles.aspx?catid=3241&pg=0" title="category">
           HtmlNodeCollection  linksToCategories = doc.DocumentNode.SelectNodes("//li/a[@title='category']");
           
           var articleCategories = new List<ArticleCategory>();
            if (linksToCategories == null) return articleCategories;
            UriBuilder u1 = new UriBuilder(blogUrl);
            u1.Path = "";
            string domain = u1.ToString().TrimEnd("/");
   
            foreach (var link in linksToCategories)
            {
                var urlCategoryArticles = link.Attributes["href"].Value;
                string catid = urlCategoryArticles.RightAfter("catid=",true);
                if (!catid.IsNullOrEmpty())
                {
                    var articleLinks = new HashSet<string>();
                    string desc = link.InnerText;
                    var articleCategory = new ArticleCategory()
                    {
                        MlCategory = new BlogMLCategory(){Approved=true,ID=catid,Description=desc,Title=desc},
                        ArticleUrls = articleLinks
                    };
                    articleCategories.Add(articleCategory);
                     int attemptsPage = 15;
                    for (int i = 0; i < attemptsPage; i++)
                    {

                        string page = domain + urlCategoryArticles + String.Format("&pg={0}", i);
                        string listContent = OpenPage(page,authentication.CookieContainer);
                        if (!listContent.IsNullOrEmpty())
                        {                             
                            string path = dirSavePath + String.Format("/Articles/Category{0}Page{1}.htm",catid, i);
                            EnsureDirectoryExists(path);
                            File.WriteAllText(path, listContent);
                            int countBefore = articleLinks.Count;
                            AddArticleLinksFromPage(listContent, articleLinks);
                            if (countBefore == articleLinks.Count)
                            {  //No new rows on the page
                                break;
                            }
                        }
                        else break;//no more pages for this category
                    }
                }
            }
            return articleCategories;
        }

        private static string OpenPage(string page, CookieContainer cookieContainer = null,
            TraceEventType eventType = TraceEventType.Warning) 
        {
            //http://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage
            string result = "";
            using (var handler = new HttpClientHandler())
            { 
                if (cookieContainer != null)
                {
                  handler.CookieContainer = cookieContainer;
                }
        
                // ... Use HttpClient.
                using (HttpClient client = new HttpClient(handler))
                {

                    using (HttpResponseMessage response = client.GetAsync(page).Result) // await client.GetAsync(page))
                    {
                        Console.WriteLine(page + " IsSuccessStatusCode " + response.IsSuccessStatusCode);
                        if (response.IsSuccessStatusCode)
                        {
                            using (HttpContent content = response.Content)
                            {
                                // ... Read the string.
                                result = content.ReadAsStringAsync().Result;

                                // ... Display the result.
                                if (result != null && (eventType == TraceEventType.Verbose))
                                {
                                    Console.WriteLine(result);
                                }
                            }
                        }
                    }
                }
            }

             return result;
        }
        private static CookieContainer OpenLoginPage(string blogUrl, SiteAuthentication authentication, TraceEventType eventType = TraceEventType.Warning) 
        {
            //http://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage
            string result = "";
           string page = blogUrl + "/login.aspx";
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            handler.AllowAutoRedirect = false;//http://stackoverflow.com/questions/10453892/how-can-i-get-system-net-http-httpclient-to-not-follow-302-redirects

            //__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUJODE5NDIyNTU3ZBgBBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WAQULY2hrUmVtZW1iZXI2Rzq4IJF%2B%2FVH%2FaC65fsYevPfDqg%3D%3D
            //&__VIEWSTATEGENERATOR=C2EE9ABB&tbUserName=&tbPassword=&btnLogin=Login
            // ... Use HttpClient.
            using (HttpClient client = new HttpClient(handler))
            {
                var postForm = new FormUrlEncodedContent(new[] 
                { 
                     new KeyValuePair<string, string>("__EVENTTARGET", ""),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
                        new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", "C2EE9ABB"),
                   
                    new KeyValuePair<string, string>("tbUserName", authentication.Username),
                    new KeyValuePair<string, string>("tbPassword", authentication.Password),
                      new KeyValuePair<string, string>("btnLogin", "Login"),
                });
                using (HttpResponseMessage response = client.PostAsync(page, postForm).Result)
                {
                    Console.WriteLine(page + " IsSuccessStatusCode " + response.IsSuccessStatusCode + " StatusCode " + response.StatusCode);
                 
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        authentication.CookieContainer = cookieContainer;
                    }
                }
            }
            PrintCookies(page, cookieContainer);

            return cookieContainer;
        }
        public static BlogMLPost ArticleToBlogMlPost(List<ArticleCategory> categories, string content, string articleUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            //<a title="View Entry" href="http://geekswithblogs.net/mnf/archive/2007/07/17/My-CollectionsHelper-class.aspx">My CollectionsHelper class</a>
            //<div class="post padTopSm2">
            var postHtml = doc.DocumentNode.SelectSingleNode("//div[@class='post padTopSm2']");
            //id="viewpost_ascx_TitleUrl"
            var titleLink = postHtml.SelectSingleNode("//a[@id='viewpost_ascx_TitleUrl']");
            var title = titleLink.InnerText;
            //<a id="viewpost_ascx_ShareLinksV2_shareLinksComment" data-disqus-identifier="geekswithblogs_postId_149555" gwb-comment-count="0">
            var editLink = postHtml.SelectSingleNode("//a[@id='viewpost_ascx_ShareLinksV2_shareLinksComment']");
            string id = editLink != null ? editLink.Attributes["data-disqus-identifier"].Value.RightAfterLast("_") : "";

            //    <span class="postFoot">
            // Posted on Monday, May 7, 2012 10:26 PM | Back to top
            var footerStr = postHtml.SelectSingleNode("//span[@class='postFoot']").InnerText;
            var strdateCreated = footerStr.RightAfter("Posted on").LeftBefore("|").Trim();
            DateTime dateCreated;
            bool valid = DateTime.TryParse(strdateCreated, out dateCreated);
            if (!valid)
            {
                Console.WriteLine("Invalid  dateCreated " + footerStr);
            }
            var postNode = postHtml;
            //below  <div class="postHdr clearfix">
            var postHdr = postHtml.SelectSingleNode("//div[@class='postHdr clearfix']");
            if (postHdr != null)
            {
                postNode.RemoveChild(postHdr, false);
            }
            //remove <span class="postFoot">
            var postFoot = postHtml.SelectSingleNode("//span[@class='postFoot']");
            if (postFoot != null)
            {
                postNode.RemoveChild(postFoot, false);
            }

            string postBody = postNode.InnerHtml;// HttpUtility.HtmlEncode(postNode.InnerHtml);
            BlogMLPost post = new BlogMLPost()
            {
                PostType = BlogPostTypes.Article,
                Title = title,
                DateCreated = dateCreated,
                DateModified = dateCreated,
                ID = id,
                PostUrl = articleUrl,
                Content = new BlogMLContent() { Text = postBody },
                Approved = true
            };
            // Add categories
            foreach (var cat in categories)
            {
                if (cat.ArticleUrls.Contains(articleUrl))
                {
                    var catRef=new BlogMLCategoryReference {Ref = cat.MlCategory.Title};
                    post.Categories.Add(catRef);
                }
            }
            return post;
        }
        private static void PrintCookies(string pageUrl,CookieContainer cookieContainer )
        {
            Uri uri = new Uri(pageUrl);
            IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(uri).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies)
                Console.WriteLine(cookie.Name + ": " + cookie.Value);
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists)
            {
                System.IO.Directory.CreateDirectory(fi.DirectoryName);
            }
        }
    }
}
