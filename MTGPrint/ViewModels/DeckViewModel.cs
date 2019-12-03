using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Linq;
using System.IO;
using System;

using Microsoft.Win32;

using Newtonsoft.Json;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Helper.UI;
using MTGPrint.Models;
using MTGPrint.Helper;

namespace MTGPrint.ViewModels
{
    public class DeckViewModel : Screen
    {
        private readonly IEventAggregator events;
        private readonly SimpleContainer container;
        private readonly LocalDataStorage localData;
        private readonly BackgroundPrinter printer;
        public DeckViewModel(SimpleContainer container)
        {
            events = container.GetInstance<IEventAggregator>();
            localData = container.GetInstance<LocalDataStorage>();
            printer = container.GetInstance<BackgroundPrinter>();
            this.container = container;

            MarkPrintedCommand = new LightCommand(() => MarkDeckPrinted(false));
            MarkNotPrintedCommand = new LightCommand(() => MarkDeckPrinted(true));

            printer.PrintFinished += delegate (object _, RunWorkerCompletedEventArgs args)
            {
                string errors, status;

                if (args.Error != null)
                {
                    errors = args.Error.Message;
                    status = "Deck could not be printed";

                    MessageBox.Show(Application.Current.MainWindow, args.Error.Message);
                }
                else
                {
                    status = "Deck printed";
                    errors = string.Empty;

                    foreach (var dc in Deck.Cards)
                        dc.CanPrint = false;
                    if (args.Result is PrintOptions po && po.OpenPDF)
                        Process.Start(new ProcessStartInfo(po.FileName) { UseShellExecute = true });
                    else
                        MessageBox.Show(Application.Current.MainWindow, "Cards printed!");
                }
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { IsWndEnabled = true, IsLoading = false, Errors = errors, Status = status });
            };

            Deck.PropertyChanged += (object o, PropertyChangedEventArgs _)
                => events.PublishOnUIThreadAsync(new UpdateStatusEvent { Info = $"card count: {Deck.CardCount} | token count: {Deck.TokenCount}" });
        }
        public Deck Deck { get; } = new Deck(false);

        public ICommand MarkPrintedCommand { get; }
        public ICommand MarkNotPrintedCommand { get; }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            bool result = true;
            OnUIThread(() =>
            {
                if (Deck.HasChanges &&
                        MessageBox.Show(Application.Current.MainWindow,
                                        "Your deck has unsaved changes! Continue anyway?",
                                        "Unsaved Changes",
                                        MessageBoxButton.YesNo)
                        == MessageBoxResult.No)
                {
                    result = false;
                }
            });
            return result;
        }

        #region Menu
        public void OpenMainMenu()
           => events.PublishOnUIThreadAsync(new CloseScreenEvent());

        public void AddCards()
        {
            events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Importing cards" });
            var vm = container.GetInstance<AddCardsViewModel>();
            var result = container.GetInstance<IWindowManager>().ShowDialogAsync(vm).Result;

            if (result == true && !string.IsNullOrEmpty(vm.ImportCards) && vm.ImportCards.Trim().Length > 0)
            {
                var parsedCards = localData.ParseCardList(vm.ImportCards.Trim(), out var errors);
                parsedCards.ForEach(dc => Deck.Cards.Add(dc));
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Cards imported", Errors = string.Join(Environment.NewLine, errors) });
            }
        }

        public void GenerateTokens()
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

        public void SaveDeckAs()
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

        public void SaveDeck()
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

        public void Print()
        {
            var vm = container.GetInstance<PrintViewModel>();
            vm.Deck = Deck;
            var result = container.GetInstance<IWindowManager>().ShowDialogAsync(vm).Result;
            if (result == true)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    InitialDirectory = Path.Combine( Constants.EXE_PATH, "prints" )
                };

                if (sfd.ShowDialog() == true)
                {
                    events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Creating PDF", IsLoading = true, IsWndEnabled = false });
                    vm.PrintOptions.FileName = sfd.FileName;
                    printer.Print(Deck, vm.PrintOptions);
                }
            }
        }

        public void ShowInfo()
            => container.GetInstance<IWindowManager>().ShowDialogAsync(container.GetInstance<InfoViewModel>()).Wait();
  
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
