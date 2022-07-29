using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Linq;
using System.IO;
using System;

using Microsoft.Win32;

using Newtonsoft.Json;

using Caliburn.Micro;

using MTGPrint.Helper;

namespace MTGPrint.Models
{
    public class Deck : INotifyPropertyChanged
    {
        private bool hasChanges;
        private string fileName;

        public Deck()
        {
            //parse constructor
        }

        public Deck(bool isTemp)
        {
            if (isTemp)
                return;

            Cards.CollectionChanged += OnCardCollectionChanged;
        }

        [JsonIgnore]
        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged(nameof(CanSave));
            }
        }

        [JsonIgnore]
        public bool HasChanges
        {
            get => hasChanges;
            set
            {
                hasChanges = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("cards")]
        public BindableCollection<DeckCard> Cards { get; set; } = new BindableCollection<DeckCard>();

        [JsonProperty("print_options")]
        public PrintOptions PrintOptions { get; set; } = new PrintOptions();

        [JsonIgnore]
        public int CardCount => Cards.Where(c => !c.IsToken && !c.IsChild).Sum(c => c.Count);

        public int UniqueCardCount => Cards.Where(c => !c.IsToken && !c.IsChild).Count();

        [JsonIgnore]
        public int TokenCount => Cards.Where(c => c.IsToken || c.IsChild).Sum(c => c.Count);

        [JsonIgnore]
        public bool CanSave => !string.IsNullOrEmpty(FileName);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnCardCollectionChanged(object sender, NotifyCollectionChangedEventArgs ne)
        {
            HasChanges = true;
            OnPropertyChanged(nameof(CardCount));
            OnPropertyChanged(nameof(TokenCount));

            if (ne.NewItems != null)
            {
                foreach (var card in ne.NewItems.Cast<DeckCard>())
                {
                    card.PropertyChanged += OnCardPropertyChanged;
                    card.DeleteRequest += OnCardRequestCardDelete;
                    card.DuplicateRequest += OnCardRequestDuplicate;
                    card.SaveArtCropRequest += OnCardRequestSaveArtCrop;
                }
            }
            if (ne.OldItems != null)
            {
                foreach (var card in ne.OldItems.Cast<DeckCard>())
                {
                    card.PropertyChanged -= OnCardPropertyChanged;
                    card.DeleteRequest -= OnCardRequestCardDelete;
                    card.DuplicateRequest -= OnCardRequestDuplicate;
                    card.SaveArtCropRequest -= OnCardRequestSaveArtCrop;
                }
            }
        }

        private void OnCardPropertyChanged(object s, PropertyChangedEventArgs pe)
        {
            if (pe.PropertyName == "SelectPrint" || pe.PropertyName == "Count" || pe.PropertyName == "CanPrint")
            {
                HasChanges = true;
                if (pe.PropertyName == "Count")
                {
                    OnPropertyChanged((s as DeckCard).IsToken ? nameof(TokenCount) : nameof(CardCount));
                }
            }
        }

        private void OnCardRequestCardDelete(DeckCard card)
        {
            var index = Cards.IndexOf(card);
            if (Cards.Count < index && Cards[index + 1].IsChild)
                Cards.RemoveAt(index + 1);

            Cards.Remove(card);
        }
        private void OnCardRequestDuplicate(DeckCard card)
        {
            var newCard = new DeckCard
            {
                Count = card.Count,
                OracleId = card.OracleId,
                Prints = card.Prints,
                SelectPrint = card.SelectPrint
            };

            Cards.Add(newCard);

            var index = Cards.IndexOf(card);
            if (Cards.Count > index && Cards[index + 1].IsChild)
            {
                var child = Cards[index + 1];
                newCard = new DeckCard
                {
                    IsChild = child.IsChild,
                    OracleId = child.OracleId
                };

                Cards.Add(newCard);
            }
        }
        private void OnCardRequestSaveArtCrop(DeckCard card)
        {
            var sfd = new SaveFileDialog
            {
                FileName = card.IsChild ? card.OracleId.ToString() : card.SelectPrint.Id.ToString(),
                Filter = "JPEG file (*.jpg)|*.jpg",
                InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "art_crops")
            };
            try
            {
                if (sfd.ShowDialog() == true)
                {
                    string dlPath;
                    if (card.IsChild)
                    {
                        var index = Cards.IndexOf(card);
                        dlPath = Cards[index - 1].SelectPrint.ChildUrls.ArtCrop;
                    }
                    else
                        dlPath = card.SelectPrint.ImageUrls.ArtCrop;

                    IoC.Get<BackgroundLoader>().RunAsync(dlPath, sfd.FileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(Application.Current.MainWindow, e.Message);
            }
        }
    }
}
