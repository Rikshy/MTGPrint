using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

namespace MTGPrint.Models
{
    public class Deck : INotifyPropertyChanged
    {
        public Deck()
        {
            //parse constructor
        }

        public Deck(bool isTemp)
        {
            if ( isTemp ) return;

            Cards.CollectionChanged += delegate ( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs ne )
            {
                HasChanges = true;
                OnPropertyChanged( nameof( HasChanges ) );
                OnPropertyChanged( nameof( CardCount ) );
                OnPropertyChanged( nameof( TokenCount ) );

                if ( ne.NewItems != null )
                {
                    foreach ( var i in ne.NewItems )
                    {
                        ( i as DeckCard ).PropertyChanged += delegate ( object s, PropertyChangedEventArgs pe )
                          {
                              if ( pe.PropertyName == "SelectPrint" || pe.PropertyName == "Count" || pe.PropertyName == "CanPrint" )
                              {
                                  HasChanges = true;
                                  OnPropertyChanged( nameof( HasChanges ) );
                                  if ( pe.PropertyName == "Count" )
                                  {
                                      if ( ( s as DeckCard ).IsToken )
                                          OnPropertyChanged( nameof( TokenCount ) );
                                      else
                                          OnPropertyChanged( nameof( CardCount ) );
                                  }
                              }
                          };
                    }
                }
            };
            Tokens.CollectionChanged += delegate 
            {
                HasTokens = Tokens.Any();
                OnPropertyChanged( nameof( HasTokens ) );
            };
        }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public bool HasChanges { get; set; }

        [JsonIgnore]
        public bool HasTokens { get; set; }

        [JsonIgnore]
        public ObservableCollection<CardParts> Tokens { get; set; } = new ObservableCollection<CardParts>();

        [JsonProperty( "version" )]
        public int Version { get; set; }

        [JsonProperty( "cards" )]
        public ObservableCollection<DeckCard> Cards { get; set; } = new ObservableCollection<DeckCard>();

        [JsonIgnore]
        public int CardCount => Cards.Where(c => !c.IsToken && !c.IsChild).Sum(c => c.Count);

        [JsonIgnore]
        public int TokenCount => Cards.Where(c => c.IsToken || c.IsChild).Sum(c => c.Count);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    public class DeckCard : INotifyPropertyChanged
    {
        [JsonProperty( "oracle_id" )]
        public Guid OracleId { get; set; }

        private Guid? selectedPrintId;
        [JsonProperty( "selected_print_id" )]
        public Guid? SelectedPrintId
        {
            get => selectedPrintId;
            set
            {
                selectedPrintId = value;
                OnPropertyChanged( nameof(SelectPrint) );
            }
        }

        private int count;
        [JsonProperty( "count" )]
        public int Count
        {
            get => count;
            set
            {
                count = value;
                OnPropertyChanged();
            }
        }

        private bool canPrint = true;
        [JsonProperty( "can_print" )]
        public bool CanPrint
        {
            get => canPrint;
            set
            {
                canPrint = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("is_child")]
        public bool IsChild { get; set; }

        [JsonProperty("is_token")]
        public bool IsToken { get; set; }

        [JsonIgnore]
        public CardPrints SelectPrint
        {
            get => Prints.FirstOrDefault( p => p.Id == SelectedPrintId );
            set => SelectedPrintId = value?.Id;
        }

        [JsonIgnore]
        public ObservableCollection<CardPrints> Prints { get; set; } = new ObservableCollection<CardPrints>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
