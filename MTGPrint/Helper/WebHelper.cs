using System.Net;
using System.IO;

namespace MTGPrint.Helper
{
    public class WebHelper
    {
        static WebHelper()
        {
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
        }

        public static string Get(string url, string contentType = "application/json", bool useGzip = false)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = contentType;
            if (useGzip)
                request.AutomaticDecompression = DecompressionMethods.GZip;
            var response = (HttpWebResponse)request.GetResponse();
            using var stream = new StreamReader(response.GetResponseStream());
            return stream.ReadToEnd();
        }
    }
}
