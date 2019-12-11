using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Linq;
using System.IO;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Helper.UI;
using MTGPrint.Helper;

namespace MTGPrint.ViewModels
{
    public class ShellViewModel : Conductor<object>, IScreen, IEventHandler
    {
        private readonly LocalDataStorage localData;
        private readonly SimpleContainer container;

        public ShellViewModel(IEventAggregator events, SimpleContainer container)
        {
            localData = container.GetInstance<LocalDataStorage>();
            this.container = container;

            events.SubscribeOnPublishedThread(this);

            localData.LocalDataUpdated += delegate
                                      {
                                          StatusText = "Localdata updated";
                                          IsEnabled = true;
                                          IsLoading = false;
                                          MessageBox.Show(Application.Current.MainWindow, "Localdata updated!");
                                      };

            var loader = container.GetInstance<BackgroundLoader>();
            loader.DownloadStarted += delegate { IsLoading = true; };
            loader.DownloadComplete += delegate (object o, RunWorkerCompletedEventArgs args)
            {
                if (args.Error != null)
                {
                    Errors = args.Error.Message;
                    MessageBox.Show(Application.Current.MainWindow, args.Error.Message);
                }

                IsLoading = false;
            };

            if ( !Directory.Exists( "decks" ) )
                Directory.CreateDirectory( "decks" );
            if ( !Directory.Exists( "prints" ) )
                Directory.CreateDirectory( "prints" );
            if ( !Directory.Exists( "art_crops" ) )
                Directory.CreateDirectory( "art_crops" );

            ActivateItemAsync(container.GetInstance<MainMenuViewModel>(), new CancellationToken());
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            ValidateInternetConnection();

            if (localData.CheckForUpdates())
            {
                StatusText = "Updating localdata";
                IsLoading = true;
                localData.UpdateBulkData();
                IsEnabled = false;
                MessageBox.Show(Application.Current.MainWindow, "The client is updating local data. This might take a while.");
            }
            else
                StatusText = "Localdata is up to date";

            await base.OnInitializeAsync(cancellationToken);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            localData.HasChanges = true;
            localData.SaveLocalData();
            return await base.CanCloseAsync(cancellationToken);
        }

        public void ValidateInternetConnection()
        {
            if (!ConnectionChecker.HasInternet())
            {
                MessageBox.Show("This application needs a working internet connection!");
                TryCloseAsync();
            }
        }

        #region Bindings
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

        private string infoText;
        public string InfoText
        {
            get => infoText;
            set
            {
                infoText = value;
                NotifyOfPropertyChange();
            }
        }


        private string errors;
        public string Errors
        {
            get => errors;
            set
            {
                errors = value;
                ErrorSymbol = string.IsNullOrEmpty(errors) ? @"..\Resources\ok.png" : @"..\Resources\warning.png";
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

        public async Task HandleAsync(OpenDeckEvent message, CancellationToken cancellationToken)
        {
            ValidateInternetConnection();
            var vm = container.GetInstance<DeckViewModel>();
            vm.LoadDeck(message.DeckPath);
            await ActivateItemAsync(vm, cancellationToken);
        }

        public async Task HandleAsync(EditLocalDataEvent message, CancellationToken cancellationToken)
        {
            ValidateInternetConnection();
            await ActivateItemAsync(container.GetInstance<LocalDataViewModel>(), cancellationToken);
        }

        public async Task HandleAsync(CreateDeckEvent message, CancellationToken cancellationToken)
        {
            var vm = container.GetInstance<DeckViewModel>();
            vm.LoadDeck(message.Cards);
            if (vm.Deck.Cards.Any())
                await ActivateItemAsync(vm, cancellationToken);
        }

        public async Task HandleAsync(CloseScreenEvent message, CancellationToken cancellationToken)
        {
            InfoText = string.Empty;
            StatusText = "Choose your Nemesis";
            await ActivateItemAsync(container.GetInstance<MainMenuViewModel>(), cancellationToken);
        }

        public async Task HandleAsync(UpdateStatusEvent message, CancellationToken cancellationToken)
        {
            Errors = message.Errors ?? Errors;
            StatusText = message.Status ?? StatusText;
            InfoText = message.Info ?? InfoText;
            IsEnabled = message.IsWndEnabled ?? IsEnabled;
            IsLoading = message.IsLoading ?? IsLoading;
        }
    }
}
