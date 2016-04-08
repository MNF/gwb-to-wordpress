using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
namespace CsConsole
{
    public class ArticlesByCategorySearch
    {
        /// <summary>
        /// In a loop call http://geekswithblogs.net/mnf/category/6248.aspx and save what found

        /// </summary>
        /// <param name="blogUrl"></param>
        /// <param name="attemptsCount"></param>
        public void TryAllArticleLinks(string blogUrl = "http://geekswithblogs.net/mnf/", int attemptsCount = 10000)
        {
            string dirSavePath=@"C:\temp\Categories\";
            for (int i = 0; i < attemptsCount; i++)
            {
                string page = blogUrl + String.Format("category/{0}.aspx", i);
                string content=OpenPage(page);
                if(!String.IsNullOrEmpty(content))
                {    //http://stackoverflow.com/questions/9907682/create-a-txt-file-if-doesnt-exist-and-if-it-does-append-a-new-line
                    string path = dirSavePath + String.Format("{0}.htm", i);
                    EnsureDirectoryExists(path);
                    TextWriter tw = new StreamWriter(path, false);
                    tw.WriteLine(content);
                    tw.Close(); 
                }
                //Task t = new Task(()=>OpenPage(page));
                //t.Start();
                //t.Wait();
            }
            Console.ReadLine();
        }

        private static string OpenPage(string page) //
        {
            string result ="";
            // ... Use HttpClient.
            using (HttpClient client = new HttpClient())
            {
               
                using (HttpResponseMessage response =client.GetAsync(page).Result)// await client.GetAsync(page))
                {
                    Console.WriteLine(page + " IsSuccessStatusCode " + response.IsSuccessStatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            // ... Read the string.
                             result = content.ReadAsStringAsync().Result;//   await content.ReadAsStringAsync();

                            // ... Display the result.
                            if (result != null)//&&                            result.Length >= 50)
                            {
                                Console.WriteLine(result);
                            }
                        }
                    }
                }
            }
            return result;
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
