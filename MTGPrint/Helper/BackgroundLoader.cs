using System;
using System.ComponentModel;
using System.Net.Http;

namespace MTGPrint.Helper
{
    public class BackgroundLoader
    {
        private readonly HttpClient loader = new();
        private readonly BackgroundWorker worker = new();
        public event EventHandler DownloadStarted;
        public event RunWorkerCompletedEventHandler DownloadComplete
        {
            add { worker.RunWorkerCompleted += value; }
            remove { worker.RunWorkerCompleted -= value; }
        }

        public BackgroundLoader()
        {
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var pair = (ValueTuple<string, string>)e.Argument;
                loader.DownloadFileAsync(new Uri(pair.Item1), pair.Item2).Start();
            };
        }

        public void RunAsync(string source, string dest)
        {
            DownloadStarted?.Invoke(this, EventArgs.Empty);
            worker.RunWorkerAsync((source, dest));
        }
    }
}
