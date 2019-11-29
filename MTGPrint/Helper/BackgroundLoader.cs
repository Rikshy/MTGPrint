using System.ComponentModel;
using System.Net;
using System;

namespace MTGPrint.Helper
{
    public class BackgroundLoader
    {
        private static readonly WebClient loader = new WebClient();
        private static readonly BackgroundWorker worker = new BackgroundWorker();
        public static event EventHandler DownloadStarted;
        public static event RunWorkerCompletedEventHandler DownloadComplete
        {
            add { worker.RunWorkerCompleted += value; }
            remove { worker.RunWorkerCompleted -= value; }
        }

        static BackgroundLoader()
        {
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var pair = (ValueTuple<string, string>)e.Argument;
                loader.DownloadFile(pair.Item1, pair.Item2);
            };
        }

        public static void RunAsync(string source, string dest)
        {
            DownloadStarted?.Invoke(null, EventArgs.Empty);
            worker.RunWorkerAsync((source, dest));
        }
    }
}
