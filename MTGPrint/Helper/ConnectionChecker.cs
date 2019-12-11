using System.Net;
using System;

namespace MTGPrint.Helper
{
    public class ConnectionChecker
    {
        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                var w = base.GetWebRequest(uri);
                w.Timeout = 20 * 1000;
                return w;
            }
        }

        public static bool HasInternet()
        {
            try
            {
                using (var client = new MyWebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
