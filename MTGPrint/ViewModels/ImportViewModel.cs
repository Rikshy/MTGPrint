using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class ImportViewModel : Screen
    {
        private readonly IEventAggregator events;

        private string importText = string.Empty;
        private string importUrl = string.Empty;
        private Method importMethod = Method.Text;

        public ImportViewModel(IEventAggregator e)
        {
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
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Importing cards" });
                ImportedCards = DecklistGrabber.GrabDecklist(ImportText, ImportMethod, out var errors);

                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Cards imported", Errors = string.Join(Environment.NewLine, errors) });
                TryCloseAsync(true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void OpenDeckstats()
            => OpenUrl("www.deckstats.net");
        public void OpenScryfall()
            => OpenUrl("www.scryfall.com");
        public void OpenGoldfish()
            => OpenUrl("www.mtggoldfish.com");
        public void OpenAetherhub()
            => OpenUrl("www.aetherhub.com");
        public void OpenTappedout()
            => OpenUrl("www.tappedout.net");
        public void OpenArchidekt()
            => OpenUrl("www.archidekt.com");

        public void OpenUrl(string url)
            => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
