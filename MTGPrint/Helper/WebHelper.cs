using System.Net;
using System.IO;
using System.Net.Http;

namespace MTGPrint.Helper
{
    public class WebHelper
    {

        public static string Get(string url, string contentType = "application/json", bool useGzip = false)
        {
            HttpClientHandler handler = new();
            handler.ServerCertificateCustomValidationCallback += (o, c, ch, er) => true;
            handler.Properties["Content-Type"] = contentType;
            if (useGzip)
                handler.AutomaticDecompression = DecompressionMethods.GZip;

            using var client = new HttpClient(handler);
            return client.GetStringAsync(url).Result;
        }
    }
}
