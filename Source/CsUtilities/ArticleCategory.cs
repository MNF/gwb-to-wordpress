using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsUtilities
{
    public class ArticleCategory
    {
        public BlogML.Xml.BlogMLCategory MlCategory { get; set; }
        public HashSet<string> ArticleUrls { get; set; }
    }
}
