using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Linq;
using System.IO;
using System;

using Microsoft.Win32;

using Newtonsoft.Json;

using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly LocalDataStorage model = new LocalDataStorage();
        public readonly string EXE_PATH = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

        public MainViewModel()
        {
            WindowLoadedCommand = new LightCommand(WindowLoaded);
            WindowClosedCommand = new LightCommand(WindowClosed);

            OpenDeckCommand = new LightCommand(OpenDeck);

            NewDeckCommand = new LightCommand(NewDeck);
            AddCardsCommand = new LightCommand(AddCards);
            GenerateTokenCommand = new LightCommand(GenerateToken);
            MarkPrintedCommand = new LightCommand(() => MarkDeckPrinted(false));
            MarkNotPrintedCommand = new LightCommand(() => MarkDeckPrinted(true));
            SaveDeckAsCommand = new LightCommand(SaveDeckAs);
            SaveDeckCommand = new LightCommand(SaveDeck);
            PrintCommand = new LightCommand(Print);
            InfoCommand = new LightCommand(ShowInfo);

            LocalDataStorage.LocalDataUpdated += delegate
                                      {
                                          StatusText = "Localdata updated";
                                          IsEnabled = true;
                                          IsLoading = false;
                                          MessageBox.Show(Application.Current.MainWindow, "Localdata updated!");
                                      };

            BackgroundPrinter.PrintFinished += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                IsEnabled = true;
                if ( args.Error != null )
                {
                    LoadErrors = args.Error.Message;
                    MessageBox.Show( Application.Current.MainWindow, args.Error.Message );
                    StatusText = "Deck could not be printed";
                }
                else
                {
                    LoadErrors = string.Empty;
                    StatusText = "Deck printed";
                    foreach ( var dc in Deck.Cards )
                        dc.CanPrint = false;
                    if ( args.Result is PrintOptions po && po.OpenPDF )
                        Process.Start( new ProcessStartInfo(po.FileName) { UseShellExecute = true });
                    else
                        MessageBox.Show( Application.Current.MainWindow, "Cards printed!" );
                }
                IsLoading = false;
            };
            BackgroundLoader.DownloadStarted += delegate { IsLoading = true; };
            BackgroundLoader.DownloadComplete += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                if ( args.Error != null )
                {
                    LoadErrors = args.Error.Message;
                    MessageBox.Show( Application.Current.MainWindow, args.Error.Message );
                }
                IsLoading = false;
            };

            if ( Application.Current.MainWindow != null )
                Application.Current.MainWindow.Closing += CanClose;

            if ( !Directory.Exists( "decks" ) )
                Directory.CreateDirectory( "decks" );
            if ( !Directory.Exists( "prints" ) )
                Directory.CreateDirectory( "prints" );
            if ( !Directory.Exists( "art_crops" ) )
                Directory.CreateDirectory( "art_crops" );
        }

        #region Bindings
        public ICommand WindowLoadedCommand { get; }
        public ICommand WindowClosedCommand { get; }

        public ICommand OpenDeckCommand { get; }
        public ICommand NewDeckCommand { get; }

        public ICommand AddCardsCommand { get; }
        public ICommand GenerateTokenCommand { get; }
        public ICommand MarkPrintedCommand { get; }
        public ICommand MarkNotPrintedCommand { get; }
        public ICommand SaveDeckAsCommand { get; }
        public ICommand SaveDeckCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand InfoCommand { get; }

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

        public Deck Deck { get; } = new Deck(false);

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
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region CommandActions
        private void WindowLoaded()
        {
            if (LocalDataStorage.CheckForUpdates())
            {
                StatusText = "Updating localdata";
                IsLoading = true;
                LocalDataStorage.UpdateBulkData();
                IsEnabled = false;
                MessageBox.Show( Application.Current.MainWindow, "The client is updating local data. This might take a while." );
            }
            else
                StatusText = "Localdata updated";
        }

        private void WindowClosed() { LocalDataStorage.SaveLocalData(); }

        private void OpenDeck()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Deck file (*.jd)|*.jd",
                InitialDirectory = Path.Combine( EXE_PATH, "decks" )
            };
            try
            {
                if ( ofd.ShowDialog() != true ) return;
                LoadDeck(ofd.FileName);

                CreateOpenGridVisibility = Visibility.Collapsed;
                DeckGridVisibility = Visibility.Visible;

                CanSave = true;
                LoadErrors = string.Empty;
            }
            catch ( Exception e )
            {
                MessageBox.Show( Application.Current.MainWindow, e.Message );
            }
        }

        #region Menu
        private void NewDeck()
        {
            if (Deck.HasChanges &&
                MessageBox.Show( Application.Current.MainWindow, "Your deck has unsaved changes! Continue anyway?",
                                                         "Unsaved Changes",
                                                         MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            Deck.Cards.Clear();
            Deck.HasChanges = false;
            CanSave = false;
            LoadErrors = string.Empty;

            CreateOpenGridVisibility = Visibility.Visible;
            DeckGridVisibility = Visibility.Collapsed;
        }

        private void AddCards()
        {
            var vm = new AddCardsViewModel();
            var addCardsView = new AddCardsView { DataContext = vm, Owner = Application.Current.MainWindow };
            if (addCardsView.ShowDialog() == true && !string.IsNullOrEmpty(vm.ImportCards) && vm.ImportCards.Trim().Length > 0)
            {
                StatusText = "Importing cards";
                var parsedCards = LocalDataStorage.ParseCardList(vm.ImportCards.Trim(), out var errors);
                parsedCards.ForEach(dc => Deck.Cards.Add(dc));
                if (Deck.Cards.Any())
                {
                    CreateOpenGridVisibility = Visibility.Collapsed;
                    DeckGridVisibility = Visibility.Visible;
                }

                LoadErrors = string.Join(Environment.NewLine, errors);
                StatusText = "Cards imported";
            }
        }

        private void GenerateToken()
        {
            foreach (var card in Deck.Cards.Where(c => c.LocalData.Parts != null))
            {
                var parts = card.LocalData.Parts.Where( p => p.Component == CardComponent.Token || p.Component == CardComponent.ComboPiece );
                foreach (var part in parts)
                {
                    if (!Deck.Cards.Any(c => c.SelectedPrintId == part.Id))
                    {
                        Deck.Cards.Add(new DeckCard
                        {
                            OracleId = card.OracleId,
                            LocalData = card.LocalData,
                            SelectPrint = card.Prints.First(cp => cp.Id == part.Id),
                            Prints = card.Prints,
                            IsToken = true,
                            Count = 5
                        });
                    }
                }
            }
        }

        private void MarkDeckPrinted(bool canPrint)
        {
            foreach ( var dc in Deck.Cards )
                dc.CanPrint = canPrint;
        }

        private void SaveDeckAs()
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
                    SaveDeck(sfd.FileName);
                    CanSave = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show( Application.Current.MainWindow, e.Message);
            }
        }

        private void SaveDeck()
        {
            try
            {
                SaveDeck(null);
            }
            catch (Exception e)
            {
                MessageBox.Show( Application.Current.MainWindow, e.Message);
            }
        }

        private void Print()
        {
            var vm = new PrintViewModel { PrintOptions = BackgroundPrinter.LoadPrintSettings() };
            var printView = new PrintView { DataContext = vm, Owner = Application.Current.MainWindow };
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
                    BackgroundPrinter.Print(Deck, vm.PrintOptions);
                }
            }
        }

        private void ShowInfo()
        {
            new InfoView { Owner = Application.Current.MainWindow }.ShowDialog();
        }
        #endregion
        #endregion

        private void SaveDeck(string path)
        {
            var savePath = string.IsNullOrEmpty(path) ? Deck.FileName : path;
            Deck.FileName = savePath;
            File.WriteAllText(savePath, JsonConvert.SerializeObject(Deck));
            Deck.HasChanges = false;
        }

        private void LoadDeck(string path)
        {
            var tempDeck = JsonConvert.DeserializeObject<Deck>(File.ReadAllText(path));
            if (!tempDeck.Cards.Any())
                throw new FileLoadException("invalid deck file");

            Deck.Cards.Clear();
            foreach (var tempCard in tempDeck.Cards)
            {
                var lcard = LocalDataStorage.LocalCards.FirstOrDefault( lc => lc.OracleId == tempCard.OracleId );

                if (!tempCard.IsChild)
                {
                    tempCard.Prints = lcard.Prints;
                    if (tempCard.SelectedPrintId == null)
                        tempCard.SelectedPrintId = lcard.DefaultPrint ?? lcard.Prints.First().Id;
                }
                tempCard.LocalData = lcard;

                Deck.Cards.Add(tempCard);
            }

            Deck.Version = tempDeck.Version;

            Deck.FileName = path;
            Deck.HasChanges = false;
        }

        private void CanClose(object sender, CancelEventArgs args)
        {
            if (Deck.HasChanges &&
                MessageBox.Show( Application.Current.MainWindow, "Your deck has unsaved changes! Continue anyway?",
                                "Unsaved Changes",
                                MessageBoxButton.YesNo)
                == MessageBoxResult.No)
                args.Cancel = true;

        }
    }
}
