using Caliburn.Micro;
using Microsoft.Win32;
using MTGPrint.EventModels;
using MTGPrint.Helper;
using MTGPrint.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MTGPrint.ViewModels
{
    public class DeckViewModel : Screen
    {
        private readonly IEventAggregator events;
        private readonly SimpleContainer container;
        private readonly LocalDataStorage localData;
        public DeckViewModel(SimpleContainer container)
        {
            events = container.GetInstance<IEventAggregator>();
            localData = container.GetInstance<LocalDataStorage>();
            this.container = container;

            OpenMainMenuCommand = new LightCommand(OpenMainMenu);
            AddCardsCommand = new LightCommand(AddCards);
            GenerateTokenCommand = new LightCommand(GenerateToken);
            MarkPrintedCommand = new LightCommand(() => MarkDeckPrinted(false));
            MarkNotPrintedCommand = new LightCommand(() => MarkDeckPrinted(true));
            SaveDeckAsCommand = new LightCommand(SaveDeckAs);
            SaveDeckCommand = new LightCommand(SaveDeck);
            PrintCommand = new LightCommand(Print);
            InfoCommand = new LightCommand(ShowInfo);
        }
        public Deck Deck { get; } = new Deck(false);

        public ICommand OpenMainMenuCommand { get; }
        public ICommand AddCardsCommand { get; }
        public ICommand GenerateTokenCommand { get; }
        public ICommand MarkPrintedCommand { get; }
        public ICommand MarkNotPrintedCommand { get; }
        public ICommand SaveDeckAsCommand { get; }
        public ICommand SaveDeckCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand InfoCommand { get; }

        #region Menu
        private void OpenMainMenu()
        {
            if (Deck.HasChanges &&
                MessageBox.Show(Application.Current.MainWindow, "Your deck has unsaved changes! Continue anyway?",
                                                         "Unsaved Changes",
                                                         MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            events.PublishOnUIThreadAsync(new CloseScreenEvent());
        }

        private void AddCards()
        {
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
            foreach (var dc in Deck.Cards)
                dc.CanPrint = canPrint;
        }

        private void SaveDeckAs()
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Deck file (*.jd)|*.jd",
                InitialDirectory = Path.Combine( Constants.EXE_PATH, "decks")
            };

            try
            {
                if (sfd.ShowDialog() == true)
                {
                    SaveDeck(sfd.FileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(Application.Current.MainWindow, e.Message);
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
                MessageBox.Show(Application.Current.MainWindow, e.Message);
            }
        }

        private void Print()
        {
            var vm = container.GetInstance<PrintViewModel>();
            var result = container.GetInstance<IWindowManager>().ShowDialogAsync(vm, this).Result;
            if (result == true)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    InitialDirectory = Path.Combine( Constants.EXE_PATH, "prints" )
                };

                if (sfd.ShowDialog() == true)
                {
                    //IsEnabled = false;
                    //IsLoading = true;
                    //StatusText = "Creating PDF";
                    vm.PrintOptions.FileName = sfd.FileName;
                    BackgroundPrinter.Print(Deck, vm.PrintOptions);
                }
            }
        }

        private void ShowInfo()
        {
            var vm = container.GetInstance<InfoViewModel>();
            container.GetInstance<IWindowManager>().ShowDialogAsync(vm, this).Wait();
        }
        #endregion

        private void SaveDeck(string path)
        {
            var savePath = string.IsNullOrEmpty(path) ? Deck.FileName : path;
            Deck.FileName = savePath;
            File.WriteAllText(savePath, JsonConvert.SerializeObject(Deck));
            Deck.HasChanges = false;
        }

        public void LoadDeck(string path)
        {
            var tempDeck = JsonConvert.DeserializeObject<Deck>(File.ReadAllText(path));
            if (!tempDeck.Cards.Any())
                throw new FileLoadException("invalid deck file");

            Deck.Cards.Clear();
            foreach (var tempCard in tempDeck.Cards)
            {
                var lcard = localData.LocalCards.FirstOrDefault( lc => lc.OracleId == tempCard.OracleId );

                if (lcard == null)
                {
                    //LoadErrors = "Failed to load some cards (custom cards?)";
                    continue;
                }

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
    }
}
