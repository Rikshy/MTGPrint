using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace MTGPrint.Helper
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> me, Action<T> action)
            => me.ToList().ForEach(entry => action(entry));

        public static void Open(this Uri me)
            => Process.Start(new ProcessStartInfo(me.ToString()) { UseShellExecute = true });

        public static async Task DownloadFileAsync(this HttpClient client, Uri uri, string FileName)
        {
            using var s = await client.GetStreamAsync(uri);
            using var fs = new FileStream(FileName, FileMode.CreateNew);
            await s.CopyToAsync(fs);
        }
    }
}
