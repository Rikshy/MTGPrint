﻿using System.Collections.Generic;
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

        private bool allowUrlImport = true;
        private string importText = string.Empty;
        private string importUrl = string.Empty;
        private GrabMethod importMethod = GrabMethod.Url;

        public ImportViewModel(IEventAggregator e)
            => events = e;

        public bool AllowUrlImport
        {
            get => allowUrlImport;
            set
            {
                allowUrlImport = value;
                NotifyOfPropertyChange();

                if (!allowUrlImport)
                    ImportMethod = GrabMethod.Text;
            }
        }

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
        public GrabMethod ImportMethod
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
                if (ImportMethod == GrabMethod.Text && !string.IsNullOrEmpty(ImportText.Trim()))
                    return true;
                else if (ImportMethod == GrabMethod.Url && !string.IsNullOrEmpty(ImportUrl.Trim()))
                    return true;
                return false;
            }
        }

        public void Import()
        {
            try
            {
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Importing cards" });
                var import = ImportMethod == GrabMethod.Text ? ImportText : ImportUrl;
                ImportedCards = DecklistGrabber.Grab(import, ImportMethod, out var errors);

                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Cards imported", Errors = string.Join(Environment.NewLine, errors) });
                TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void OpenDeckstats()
            => new Uri("http://www.deckstats.net").Open();
        public void OpenScryfall()
            => new Uri("http://www.scryfall.com").Open();
        public void OpenGoldfish()
            => new Uri("http://www.mtggoldfish.com").Open();
        public void OpenAetherhub()
            => new Uri("http://www.aetherhub.com").Open();
        public void OpenTappedout()
            => new Uri("http://www.tappedout.net").Open();
        public void OpenArchidekt()
            => new Uri("http://www.archidekt.com").Open();
        public void OpenMoxfield()
            => new Uri("http://www.moxfield.com").Open();
        public void OpenMTGDecks()
            => new Uri("http://www.mtgdecks.net").Open();
    }
}
