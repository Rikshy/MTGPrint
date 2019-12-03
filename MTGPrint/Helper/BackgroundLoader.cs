using System.ComponentModel;
using System.Net;
using System;

namespace MTGPrint.Helper
{
    public class BackgroundLoader
    {
        private readonly WebClient loader = new WebClient();
        private readonly BackgroundWorker worker = new BackgroundWorker();
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
                loader.DownloadFile(pair.Item1, pair.Item2);
            };
        }

        public void RunAsync(string source, string dest)
        {
            DownloadStarted?.Invoke(this, EventArgs.Empty);
            worker.RunWorkerAsync((source, dest));
        }
    }
}
