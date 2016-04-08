using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using HtmlAgilityPack;
using Common;
using CsUtilities;

namespace CsConsole
{
    public class ArticlesFromAdminTests
    {
    
        public void LoadAnArtileAsPost()
        {
             string path = @"C:\MNF\Projects\gwbtowp\Source\CsConsole\SamplePages\my-listofstringshelper--class.aspx.html";
            string content = File.ReadAllText(path);
           var post= ArticlesFromAdmin.ArticleToBlogMlPost(new List<ArticleCategory>(), content,"my-listofstringshelper--class.aspx");
        }

     
    }
}
