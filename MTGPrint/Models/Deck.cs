using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTGPrint.Models
{
    public class Deck
    {
        [JsonIgnore]
        public string FileName { get; set; }
        [JsonIgnore]
        public bool HasChanges { get; set; }

        [JsonProperty( "cards" )]
        public ObservableCollection<DeckCard> Cards { get; set; } = new ObservableCollection<DeckCard>();
    }

    public class DeckCard : INotifyPropertyChanged
    {
        [JsonProperty( "oracle_id" )]
        public Guid OracleId { get; set; }

        private CardPrints print;
        [JsonProperty( "selected_print" )]
        public CardPrints SelectPrint
        {
            get => print;
            set
            {
                print = value;
                OnPropertyChanged();
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
                CountChanged?.Invoke( this, EventArgs.Empty );
            }
        }

        [JsonProperty( "is_child" )]
        public bool IsChild { get; set; }

        [JsonIgnore]
        public ObservableCollection<CardPrints> Prints { get; set; } = new ObservableCollection<CardPrints>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public static event EventHandler CountChanged;
    }
}
