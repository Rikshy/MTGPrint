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
            CreateDeckCommand = new DelegateCommand(CreateDeck);
            OpenDeckCommand = new DelegateCommand(OpenDeck);
            WindowLoadedCommand = new DelegateCommand(WindowLoaded);

            model.WorkFinished += delegate { IsEnabled = true; };

            model.UpdateBulkData();

            int i = 0;
        }


        public ICommand OpenDeckCommand { get; }
        public ICommand CreateDeckCommand { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  
        }

        private void CreateDeck(object o)
        {
            //IsEnabled = false;
            var vm = new ViewModels.AddCardsViewModel();
            var addCardsView = new AddCardsView { DataContext = vm };
            if ( addCardsView.ShowDialog() == true && vm.ImportCards.Trim().Length > 0 )
            {
                var deckCards = model.ParseCardList( vm.ImportCards.Trim(), out var errors );
                if (deckCards.Any())
                {
                    var tmpDeck = new Deck();
                    deckCards.ForEach( dc => tmpDeck.Cards.Add( dc ) );
                    //model.LoadDeckPrints( tmpDeck );
                    Deck = tmpDeck;

                    CreateOpenGridVisibility = Visibility.Collapsed;
                    DeckGridVisibility = Visibility.Visible;
                }
            }
        }

        private void OpenDeck(object o)
        {
            CreateOpenGridVisibility = Visibility.Collapsed;
        }
    }
}
