using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.IO;
using System;

using Microsoft.Win32;

using Newtonsoft.Json;

using MTGPrint.Helper;
using MTGPrint.Models;
using MTGPrint.Views;
using Caliburn.Micro;
using MTGPrint.EventModels;
using System.Threading;
using System.Threading.Tasks;

namespace MTGPrint.ViewModels
{
    public class ShellViewModel : Conductor<object>, IHandle<OpenDeckEvent>, IHandle<EditLocalDataEvent>, IHandle<CreateDeckEvent>, IHandle<CloseScreenEvent>
    {
        private LocalDataStorage localData;
        private readonly SimpleContainer container;

        public ShellViewModel(LocalDataStorage localData, IEventAggregator events, SimpleContainer container)
        {
            this.localData = localData;
            this.container = container;

            events.SubscribeOnPublishedThread(this);

            WindowLoadedCommand = new LightCommand(WindowLoaded);
            WindowClosedCommand = new LightCommand(WindowClosed);


            localData.LocalDataUpdated += delegate
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

            ActivateItemAsync(container.GetInstance<MainMenuViewModel>(), new CancellationToken());
        }

        #region Bindings
        public ICommand WindowLoadedCommand { get; }
        public ICommand WindowClosedCommand { get; }


        public bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                NotifyOfPropertyChange();
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
                NotifyOfPropertyChange();
            }
        }

        private string loadErrors;

        public string LoadErrors
        {
            get => loadErrors;
            set
            {
                loadErrors = value;
                ErrorSymbol = string.IsNullOrEmpty(loadErrors) ? @"..\Resources\ok.png" : @"..\Resources\warning.png";
                NotifyOfPropertyChange();
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                NotifyOfPropertyChange();
            }
        }

        private string errorSymbol = @"..\Resources\ok.png";
        public string ErrorSymbol
        {
            get => errorSymbol;
            set
            {
                errorSymbol = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion


        #region CommandActions
        private void WindowLoaded()
        {
            if (localData.CheckForUpdates())
            {
                StatusText = "Updating localdata";
                IsLoading = true;
                localData.UpdateBulkData();
                IsEnabled = false;
                MessageBox.Show( Application.Current.MainWindow, "The client is updating local data. This might take a while." );
            }
            else
                StatusText = "Localdata updated";
        }

        private void WindowClosed() { localData.SaveLocalData(); }


        #endregion

        private void CanClose(object sender, CancelEventArgs args)
        {
            if (Deck.HasChanges &&
                MessageBox.Show( Application.Current.MainWindow, "Your deck has unsaved changes! Continue anyway?",
                                "Unsaved Changes",
                                MessageBoxButton.YesNo)
                == MessageBoxResult.No)
                args.Cancel = true;

        }

        public async Task HandleAsync(OpenDeckEvent message, CancellationToken cancellationToken)
        {
            var vm = container.GetInstance<DeckViewModel>();
            vm.LoadDeck(message.DeckPath);
            await ActivateItemAsync(vm, cancellationToken);
        }

        public async Task HandleAsync(EditLocalDataEvent message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(container.GetInstance<LocalDataViewModel>(), cancellationToken);
        }

        public async Task HandleAsync(CreateDeckEvent message, CancellationToken cancellationToken)
        {
            var vm = container.GetInstance<AddCardsViewModel>();
            var result = await container.GetInstance<IWindowManager>().ShowDialogAsync(vm, this);

            if (result == true && !string.IsNullOrEmpty(vm.ImportCards) && vm.ImportCards.Trim().Length > 0)
            {
                StatusText = "Importing cards";
                var parsedCards = localData.ParseCardList(vm.ImportCards.Trim(), out var errors);
                parsedCards.ForEach(dc => Deck.Cards.Add(dc));
                if (Deck.Cards.Any())
                    await ActivateItemAsync(container.GetInstance<DeckViewModel>(), cancellationToken);

                LoadErrors = string.Join(Environment.NewLine, errors);
                StatusText = "Cards imported";
            }
        }

        public async Task HandleAsync(CloseScreenEvent message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(container.GetInstance<MainMenuViewModel>(), cancellationToken);
        }
    }
}
