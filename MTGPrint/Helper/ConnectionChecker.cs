using System;
using System.Net.Http;

namespace MTGPrint.Helper
{
    public class ConnectionChecker
    {
        public static bool HasInternet()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                using (client.GetAsync("http://google.com/generate_204").Result)
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
