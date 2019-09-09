using System;

using MTGPrint.Models;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using MTGPrint.Helper;

namespace MTGPrint.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MainModel model = new MainModel();
        private readonly string EXE_PATH = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

        public MainViewModel()
        {
            NewDeckCommand = new DelegateCommand(NewDeck);
            OpenDeckCommand = new DelegateCommand(OpenDeck);
            WindowLoadedCommand = new DelegateCommand(WindowLoaded);
            AddCardsCommand = new DelegateCommand(AddCards);
            SaveDeckAsCommand = new DelegateCommand(SaveDeckAs);
            SaveDeckCommand = new DelegateCommand( SaveDeck );
            PrintCommand = new DelegateCommand( Print );
            GenerateTokenCommand = new DelegateCommand( GenerateToken );
            SaveArtCommand = new DelegateCommand( SaveArt );
            DuplicardCommand = new DelegateCommand( Duplicate );
            RemoveCardCommand = new DelegateCommand( RemoveCard );

            model.LocalDataUpdated += delegate
                                      {
                                          StatusText = "Localdata updated";
                                          IsEnabled = true;
                                          IsLoading = false;
                                          MessageBox.Show("Localdata updated!");
                                      };

            model.PrintFinished += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                IsEnabled = true;
                if ( args.Error != null )
                {
                    LoadErrors = args.Error.Message;
                    MessageBox.Show( args.Error.Message );
                    StatusText = "Deck could not be printed";
                }
                else
                {
                    LoadErrors = string.Empty;
                    StatusText = "Deck printed";
                    if (args.Result is PrintOptions po && po.OpenPDF)
                        Process.Start( po.FileName );
                }
                IsLoading = false;
            };

            model.ArtDownloaded += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                if ( args.Error != null )
                {
                    LoadErrors = args.Error.Message;
                    MessageBox.Show( args.Error.Message );
                }
                IsLoading = false;
            };

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Closing += CanClose;

            if ( !Directory.Exists( "decks" ) )
                Directory.CreateDirectory( "decks" );
            if ( !Directory.Exists( "prints" ) )
                Directory.CreateDirectory( "prints" );
            if ( !Directory.Exists( "art_crops" ) )
                Directory.CreateDirectory( "art_crops" );

            DeckCard.CountChanged += delegate 
            {
                Deck.HasChanges = true;
                CardCount = Deck.Cards.Sum( c => c.Count );
            };
        }

        #region Bindings
        public ICommand NewDeckCommand { get; }
        public ICommand OpenDeckCommand { get; }
        public ICommand WindowLoadedCommand { get; }
        public ICommand AddCardsCommand { get; }
        public ICommand SaveDeckAsCommand { get; }
        public ICommand SaveDeckCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand GenerateTokenCommand { get; }
        public ICommand SaveArtCommand { get; }
        public ICommand DuplicardCommand { get; }
        public ICommand RemoveCardCommand { get; }

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

        public Deck Deck => model.Deck;

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

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
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

        public bool canSave = false;
        public bool CanSave
        {
            get => canSave;
            set
            {
                canSave = value;
                OnPropertyChanged();
            }
        }

        public bool hasTokens = false;
        public bool HasTokens
        {
            get => hasTokens;
            set
            {
                hasTokens = value;
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
                IsLoading = true;
                model.UpdateBulkData();
                IsEnabled = false;
                MessageBox.Show("The client is updating local data. This might take a while.");
            }
            else
                StatusText = "Localdata updated";
        }

        private void NewDeck(object o)
        {
            if (model.Deck.HasChanges &&
                MessageBox.Show("Your deck has unsaved changes! Continue anyway?",
                                                         "Unsaved Changes",
                                                         MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            Deck.Cards.Clear();
            Deck.Tokens.Clear();
            CardCount = 0;
            Deck.HasChanges = false;
            CanSave = false;
            HasTokens = false;
            LoadErrors = string.Empty;

            CreateOpenGridVisibility = Visibility.Visible;
            DeckGridVisibility = Visibility.Collapsed;
        }

        private void AddCards(object o)
        {
            var vm = new AddCardsViewModel();
            var addCardsView = new AddCardsView { DataContext = vm };
            if (addCardsView.ShowDialog() == true && !string.IsNullOrEmpty(vm.ImportCards) && vm.ImportCards.Trim().Length > 0)
            {
                StatusText = "Importing cards";
                model.AddCardsToDeck(vm.ImportCards.Trim(), out var errors);
                if (Deck.Cards.Any())
                {
                    CreateOpenGridVisibility = Visibility.Collapsed;
                    DeckGridVisibility = Visibility.Visible;
                }

                CardCount = Deck.Cards.Sum(c => c.Count);
                LoadErrors = string.Join(Environment.NewLine, errors);
                StatusText = "Cards imported";
                HasTokens = Deck.Tokens.Any();
            }
        }

        private void SaveDeckAs(object o)
        {
            var sfd = new SaveFileDialog
                      {
                                  Filter = "Deck file (*.jd)|*.jd",
                                  InitialDirectory = Path.Combine( EXE_PATH, "decks")
                      };
            try
            {
                if (sfd.ShowDialog() == true)
                {
                    model.SaveDeck( sfd.FileName );
                    CanSave = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void SaveDeck(object o)
        {
            try
            {
                model.SaveDeck(null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OpenDeck(object o)
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Deck file (*.jd)|*.jd",
                InitialDirectory = Path.Combine( EXE_PATH, "decks" )
            };
            try
            {
                if (ofd.ShowDialog() != true) return;
                model.OpenDeck(ofd.FileName);

                CreateOpenGridVisibility = Visibility.Collapsed;
                DeckGridVisibility = Visibility.Visible;

                CanSave = true;
                CardCount = Deck.Cards.Sum(c => c.Count);
                HasTokens = Deck.Tokens.Any();
                LoadErrors = string.Empty;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Print(object o)
        {
            var vm = new ViewModels.PrintViewModel { PrintOptions = model.LoadPrintSettings() };
            var printView = new PrintView() { DataContext = vm };
            if ( printView.ShowDialog() == true)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    InitialDirectory = Path.Combine( EXE_PATH, "prints" )
                };

                if ( sfd.ShowDialog() == true )
                {
                    IsEnabled = false;
                    IsLoading = true;
                    StatusText = "Creating PDF";
                    vm.PrintOptions.FileName = sfd.FileName;
                    model.Print( vm.PrintOptions );
                }
            }
        }

        private void GenerateToken(object o)
        {
            model.GenerateTokens();
        }

        private void SaveArt(object o)
        {
            if ( o is DeckCard card )
            {
                var sfd = new SaveFileDialog
                {
                    FileName = card.IsChild ? card.OracleId.ToString() : card.SelectPrint.Id.ToString(),
                    Filter = "JPEG file (*.jpg)|*.jpg",
                    InitialDirectory = Path.Combine( EXE_PATH, "art_crops" )
                };
                try
                {
                    if ( sfd.ShowDialog() == true )
                    {
                        IsLoading = true;
                        model.SaveArtCrop( card, sfd.FileName );
                    }
                }
                catch ( Exception e )
                {
                    MessageBox.Show( e.Message );
                }
            }
        }

        private void Duplicate(object o)
        {
            if ( o is DeckCard card )
            {
                model.DuplicateCard( card );
                CardCount = Deck.Cards.Sum( c => c.Count );
            }
        }

        private void RemoveCard(object o)
        {
            if (o is DeckCard card)
            {
                model.RemoveCardFromDeck( card );
                CardCount = Deck.Cards.Sum( c => c.Count );
                if ( CardCount == 0 )
                {
                    if ( !string.IsNullOrEmpty( Deck.FileName ) )
                    {
                        try
                        {
                            if ( MessageBox.Show( "Do you want to delete current deck from disk?", string.Empty, MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                                File.Delete( Deck.FileName );
                        }
                        catch
                        {
                            MessageBox.Show( "Could not delete deck." );
                        }
                    }

                    Deck.HasChanges = false;
                    NewDeck( null );
                }
            }
        }
        #endregion

        private void CanClose(object sender, CancelEventArgs args)
        {
            if (model.Deck.HasChanges &&
                MessageBox.Show("Your deck has unsaved changes! Continue anyway?",
                                "Unsaved Changes",
                                MessageBoxButton.YesNo)
                == MessageBoxResult.No)
                args.Cancel = true;

        }
    }
}
