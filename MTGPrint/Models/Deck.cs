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

        [JsonProperty( "cards" )]
        public ObservableCollection<DeckCard> Cards { get; set; } = new ObservableCollection<DeckCard>();
    }

    public class DeckCard : INotifyPropertyChanged
    {
        [JsonProperty( "oracle_id" )]
        public Guid OracleId { get; set; }

        //private DeckPrint print;
        //[JsonProperty( "selected_print" )]
        //public DeckPrint SelectPrint
        //{
        //    get => print;
        //    set
        //    {
        //        print = value;
        //        OnPropertyChanged();
        //    }
        //}

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
            }
        }

        [JsonIgnore]
        public ObservableCollection<CardPrints> Prints { get; set; } = new ObservableCollection<CardPrints>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    public class DeckPrint
    {
        [JsonProperty( "id" )]
        public Guid Id { get; set; }
        [JsonProperty( "oracle_id" )]
        public Guid OracleId { get; set; }

        [JsonProperty( "set" )]
        public string Set { get; set; }

        [JsonProperty( "set_name" )]
        public string SetName { get; set; }

        public string Png => $@"data\prints\{OracleId}\{Id}\png.png";
        public string Border => $@"data\prints\{OracleId}\{Id}\border.jpg";
        public string Art => $@"data\prints\{OracleId}\{Id}\art.jpg";

        public override string ToString() { return SetName; }
    }
}
