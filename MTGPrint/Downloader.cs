using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MTGPrint
{
    public class Downloader
    {
        private WebClient client = new WebClient();
        private BackgroundWorker worker = new BackgroundWorker();
        private ConcurrentQueue<DlItem> queue = new ConcurrentQueue<DlItem>();

        public Downloader()
        {
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += OnWorkerComplete;
        }

        private void OnWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            DownloadsCompleted?.Invoke( this, EventArgs.Empty );
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while ( queue.TryDequeue( out var item ) )
            {
                if ( e.Cancel )
                    break;

                client.DownloadFile( item.Source, item.Destination );
            }
        }

        public bool IsBusy => worker.IsBusy;
        public event EventHandler DownloadsCompleted;

        public void QueueDownload(string source, string dest)
        {
            queue.Enqueue( new DlItem { Source = source, Destination = dest } );
            if ( !IsBusy )
                worker.RunWorkerAsync();
        }

        private class DlItem
        {
            public string Source { get; set; }
            public string Destination { get; set; }
        }
    }
}
