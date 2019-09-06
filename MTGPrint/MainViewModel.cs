using System;

using MTGPrint.Models;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MTGPrint
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MainModel model = new MainModel();

        public MainViewModel()
        {
            OpenDeckCommand = new DelegateCommand(OpenDeck);
            WindowLoadedCommand = new DelegateCommand(WindowLoaded);
            AddCardsCommand = new DelegateCommand(AddCards);

            model.LocalDataUpdated += delegate
                                      {
                                          StatusText = "Localdata updated";
                                          IsEnabled = true;
                                          MessageBox.Show("Localdata updated!");
                                      };
        }

        #region Bindings
        public ICommand OpenDeckCommand { get; }
        public ICommand WindowLoadedCommand { get; }
        public ICommand AddCardsCommand { get; }

        private Visibility createOpenGridVisibility = Visibility.Visible;
        public Visibility CreateOpenGridVisibility
        {
            get => createOpenGridVisibility;
            set
            {
                createOpenGridVisibility = value;
                OnPropertyChanged();
            }
        } 

        private Visibility deckGridVisibility = Visibility.Collapsed;
        public Visibility DeckGridVisibility
        {
            get => deckGridVisibility;
            set
            {
                deckGridVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        private Deck deck;
        public Deck Deck
        {
            get => deck;
            set
            {
                deck = value;
                OnPropertyChanged();
            }
        }

        private string statusText;
        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        private string loadErrors;

        public string LoadErrors
        {
            get => loadErrors;
            set
            {
                loadErrors = value;
                ErrorSymbol = string.IsNullOrEmpty(loadErrors) ? @"Resources\ok.png" : @"Resources\warning.png";
                OnPropertyChanged();
            }
        }

        private string errorSymbol = @"Resources\ok.png";
        public string ErrorSymbol
        {
            get => errorSymbol;
            set
            {
                errorSymbol = value;
                OnPropertyChanged();
            }
        }

        private int cardCount = 0;

        public int CardCount
        {
            get => cardCount;
            set
            {
                cardCount = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
        }

        #region CommandActions
        private void WindowLoaded(object o)
        {
            if (model.CheckForUpdates())
            {
                StatusText = "Updating localdata";
                model.UpdateBulkData();
                IsEnabled = false;
                MessageBox.Show("The client is updating local data. This might take a while.");
            }
            else
                StatusText = "Localdata updated";
        }
        private void AddCards(object o)
        {
            var vm = new ViewModels.AddCardsViewModel();
            var addCardsView = new AddCardsView { DataContext = vm };
            if ( addCardsView.ShowDialog() == true && vm.ImportCards.Trim().Length > 0 )
            {
                var deckCards = model.ParseCardList( vm.ImportCards.Trim(), out var errors );
                if (deckCards.Any())
                {
                    var tmpDeck = Deck ?? new Deck();
                    deckCards.ForEach( dc => tmpDeck.Cards.Add( dc ) );
                    Deck = tmpDeck;
                    CardCount = Deck.Cards.Sum( c => c.Count);

                    CreateOpenGridVisibility = Visibility.Collapsed;
                    DeckGridVisibility = Visibility.Visible;
                }

                LoadErrors = string.Join(Environment.NewLine, errors);
            }
        }

        private void OpenDeck(object o)
        {
            CreateOpenGridVisibility = Visibility.Collapsed;
        }
        #endregion
    }
}
