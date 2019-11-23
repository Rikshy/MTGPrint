﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MTGPrint.Models
{
    public class Deck : INotifyPropertyChanged
    {
        [JsonIgnore]
        public string FileName { get; set; }

        private bool hasChanges = false;
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

        [JsonProperty( "version" )]
        public int Version { get; set; }

        [JsonProperty( "tokens" )]
        public List<CardParts> Tokens { get; set; } = new List<CardParts>();

        [JsonProperty( "cards" )]
        public ObservableCollection<DeckCard> Cards { get; set; } = new ObservableCollection<DeckCard>();

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
                OnPropertyChanged( "SelectPrint" );
                ArtChanged?.Invoke( this, EventArgs.Empty );
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

        [JsonProperty( "is_child" )]
        public bool IsChild { get; set; }

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

        public static event EventHandler CountChanged;
        public static event EventHandler ArtChanged;
    }
}
