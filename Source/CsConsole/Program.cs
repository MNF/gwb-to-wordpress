using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new ArticlesFromAdminTests().LoadAnArtileAsPost();
            return;
            var articlesSearch = new ArticlesByCategorySearch();
            articlesSearch.TryAllArticleLinks();
        }
    }
}
