﻿using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System;

using Newtonsoft.Json;

using Caliburn.Micro;

using MTGPrint.Helper.UI;
using MTGPrint.Helper;

namespace MTGPrint.Models
{
    public class DeckCard : INotifyPropertyChanged
    {
        public DeckCard()
        {
            OpenScryfallCommand = new LightCommand(() => LocalData.ScryUrl.Open());
            CanPrintCommand = new LightCommand(() => CanPrint = !CanPrint);
            RemoveCardCommand = new LightCommand(() => DeleteRequest?.Invoke(this));
            DuplicardCommand = new LightCommand(() => DuplicateRequest?.Invoke(this));
            MarkArtDefaultCommand = new LightCommand(() => LocalData.DefaultPrint = SelectedPrintId);
            SaveArtCommand = new LightCommand(() => SaveArtCropRequest?.Invoke(this));
        }

        [JsonIgnore]
        public LocalCard LocalData { get; set; }

        [JsonProperty("oracle_id")]
        public Guid OracleId { get; set; }

        private Guid? selectedPrintId;
        [JsonProperty("selected_print_id")]
        public Guid? SelectedPrintId
        {
            get => selectedPrintId;
            set
            {
                selectedPrintId = value;
                OnPropertyChanged(nameof(SelectPrint));
            }
        }

        private int count;
        [JsonProperty("count")]
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
        [JsonProperty("can_print")]
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
        public CardPrint SelectPrint
        {
            get => Prints.FirstOrDefault(p => p.Id == SelectedPrintId);
            set => SelectedPrintId = value?.Id;
        }

        [JsonIgnore]
        public BindableCollection<CardPrint> Prints { get; set; } = new BindableCollection<CardPrint>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [JsonIgnore]
        public ICommand OpenScryfallCommand { get; }
        [JsonIgnore]
        public ICommand CanPrintCommand { get; }
        [JsonIgnore]
        public ICommand RemoveCardCommand { get; }
        [JsonIgnore]
        public ICommand DuplicardCommand { get; }
        [JsonIgnore]
        public ICommand MarkArtDefaultCommand { get; }
        [JsonIgnore]
        public ICommand SaveArtCommand { get; }

        public event DeckCardEventHandler DeleteRequest;
        public event DeckCardEventHandler DuplicateRequest;
        public event DeckCardEventHandler SaveArtCropRequest;
    }

    public delegate void DeckCardEventHandler(DeckCard card);
}
