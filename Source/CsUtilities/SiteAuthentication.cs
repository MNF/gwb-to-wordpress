using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CsUtilities
{
    public class SiteAuthentication
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public CookieContainer CookieContainer { get; set; }

    }
}
