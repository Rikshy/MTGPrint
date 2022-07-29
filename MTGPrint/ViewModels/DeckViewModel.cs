using System.Collections.Generic;
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

using GongSolutions.Wpf.DragDrop;

using MTGPrint.EventModels;
using MTGPrint.Helper.UI;
using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class DeckViewModel : Screen, IDropTarget
    {
        private readonly IWindowManager winMan;
        private readonly IEventAggregator events;
        private readonly BackgroundPrinter printer;
        private readonly LocalDataStorage localData;

        public DeckViewModel(SimpleContainer container)
        {
            winMan = container.GetInstance<IWindowManager>();
            events = container.GetInstance<IEventAggregator>();
            printer = container.GetInstance<BackgroundPrinter>();
            localData = container.GetInstance<LocalDataStorage>();

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
                    if (args.Result is string)
                        Process.Start(new ProcessStartInfo(args.Result.ToString()) { UseShellExecute = true });
                    else
                        MessageBox.Show(Application.Current.MainWindow, "Cards printed!");
                }

                events.PublishOnUIThreadAsync(new UpdateStatusEvent { IsWndEnabled = true, IsLoading = false, Errors = errors, Status = status });
            };

            Deck.PropertyChanged += (object o, PropertyChangedEventArgs _)
                => events.PublishOnUIThreadAsync(new UpdateStatusEvent { Info = $"{Deck.CardCount} Cards // {Deck.UniqueCardCount} Unique // {Deck.TokenCount} Tokens" });
        }
        public Deck Deck { get; } = new Deck(false);

        public ICommand MarkPrintedCommand { get; }
        public ICommand MarkNotPrintedCommand { get; }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
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
            });
        }

        #region Menu
        public void OpenMainMenu()
           => events.PublishOnUIThreadAsync(new CloseScreenEvent());

        public void AddCards()
        {
            var vm = IoC.Get<ImportViewModel>();
            vm.AllowUrlImport = false;
            var result = winMan.ShowDialogAsync(vm).Result;
            
            if (result == true)
                LoadDeck(vm.ImportedCards);
        }

        public void GenerateTokens()
        {
            var tmp = Deck.Cards.Where(c => c.LocalData.Parts != null && !c.IsToken).ToList();
            foreach (var cardWithToken in tmp)
            {
                var parts = cardWithToken.LocalData.Parts.Where( p => p.Component == CardComponent.Token || p.Component == CardComponent.ComboPiece );
                var tokensToAdd = new List<DeckCard>();

                foreach (var part in parts)
                {
                    if (!Deck.Cards.Any(c => c.SelectedPrintId == part.Id))
                    {
                        var card = localData.LocalCards.FirstOrDefault( c => c.Prints.Any( cp => cp.Id == part.Id ) );
                        if (cardWithToken.OracleId == card.OracleId || tokensToAdd.Any(dc => dc.OracleId == card.OracleId))
                            continue;

                        tokensToAdd.Add(new DeckCard
                        {
                            OracleId = card.OracleId,
                            LocalData = card,
                            SelectedPrintId = card.Prints.First(cp => cp.Id == part.Id).Id,
                            Prints = card.Prints,
                            IsToken = true,
                            Count = 5
                        });
                    }
                }

                //Need to add them single to register events
                tokensToAdd.ForEach(t => Deck.Cards.Add(t));
            }
        }

        private void MarkDeckPrinted(bool canPrint)
            => Deck.Cards.ForEach(dc => dc.CanPrint = canPrint);

        public void SaveDeckAs()
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Deck file (*.jd)|*.jd",
                InitialDirectory = Path.Combine( Environment.CurrentDirectory, "decks")
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
            var vm = IoC.Get<PrintViewModel>();
            vm.PrintOptions = Deck.PrintOptions;
            var result = winMan.ShowDialogAsync(vm).Result;
            if (result == true)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    InitialDirectory = Path.Combine( Environment.CurrentDirectory, "prints" )
                };

                if (sfd.ShowDialog() == true)
                {
                    events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Creating PDF", IsLoading = true, IsWndEnabled = false });
                    printer.Print(sfd.FileName, Deck);
                }
            }
        }

        public void ShowInfo()
            => winMan.ShowDialogAsync(IoC.Get<InfoViewModel>()).Wait();  
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
                    events.PublishOnUIThreadAsync(new UpdateStatusEvent { Errors = "Failed to load some cards (custom cards?)" });
                    continue;
                }

                if (!tempCard.IsChild)
                {
                    tempCard.Prints.AddRange( lcard.Prints );
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

        public void LoadDeck(IEnumerable<DeckCard> cards)
            => cards.ForEach(dc => Deck.Cards.Add(dc));

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DeckCard && dropInfo.TargetItem is DeckCard)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }

        }
        public void Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as DeckCard;
            var target = dropInfo.TargetItem as DeckCard;

            Deck.Cards.Move(Deck.Cards.IndexOf(source), Deck.Cards.IndexOf(target));
        }
    }
}
