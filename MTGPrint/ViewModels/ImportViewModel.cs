using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Net;
using System.IO;
using System;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class ImportViewModel : Screen
    {
        private readonly LocalDataStorage localData;
        private readonly IEventAggregator events;

        private string importText = string.Empty;
        private string importUrl = string.Empty;
        private Method importMethod = Method.Text;

        public ImportViewModel(LocalDataStorage ld, IEventAggregator e)
        {
            localData = ld;
            events = e;
        }

        public bool AllowUrlImport { get; set; } = true;

        public string ImportText
        {
            get => importText;
            set
            {
                importText = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanImport));
            }
        }
        public string ImportUrl
        {
            get => importUrl;
            set
            {
                importUrl = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanImport));
            }
        }
        public Method ImportMethod 
        { 
            get => importMethod;
            set
            {
                importMethod = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanImport));
            }
        }
        public IEnumerable<DeckCard> ImportedCards { get; private set; }

        public bool CanImport
        {
            get
            {
                if (ImportMethod == Method.Text && !string.IsNullOrEmpty(ImportText.Trim()))
                    return true;
                else if (ImportMethod == Method.Url && !string.IsNullOrEmpty(ImportUrl.Trim()))
                    return true;
                return false;
            }
        }

        public void Import()
        {
            try
            {
                List<string> errors;
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Importing cards" });
                if (ImportMethod == Method.Text)
                {
                    ImportedCards = localData.ParseCardList(ImportText, out errors);
                }
                else if (ImportMethod == Method.Url)
                {
                    ImportedCards = localData.ParseCardList(CardListFromUrl(), out errors);
                }
                else
                    throw new ApplicationException("oO");

                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Cards imported", Errors = string.Join(Environment.NewLine, errors) });
                TryCloseAsync(true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string CardListFromUrl()
        {
            HttpWebRequest request = null;
            if (ImportUrl.StartsWith("https://deckstats.net"))
                request = (HttpWebRequest) WebRequest.Create($"{ImportUrl}?export_dec=1");
            if (ImportUrl.StartsWith("https://scryfall.com"))
            {
                var url = ImportUrl;

                var idx = ImportUrl.LastIndexOf('?');
                if (idx > 0)
                    url = url.Substring(0, idx);

                idx = ImportUrl.LastIndexOf('/');
                url = url.Substring(idx + 1);
                request = (HttpWebRequest)WebRequest.Create($"https://api.scryfall.com/decks/{url}/export/text");
            }

            request.Method = WebRequestMethods.Http.Get;
            var response = (HttpWebResponse)request.GetResponse();
            using var stream = new StreamReader(response.GetResponseStream());
            return stream.ReadToEnd();
        }

        public void OpenDeckstats()
            => OpenUrl("www.deckstats.net");
        public void OpenScryfall()
            => OpenUrl("www.scryfall.com");

        public void OpenUrl(string url)
            => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
