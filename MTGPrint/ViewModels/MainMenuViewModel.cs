using System.Windows;
using System.IO;
using System;

using Microsoft.Win32;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Helper;

namespace MTGPrint.ViewModels
{
    public class MainMenuViewModel : Screen
    {
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator events;

        public MainMenuViewModel()
        {
            windowManager = IoC.Get<IWindowManager>();
            events = IoC.Get<IEventAggregator>();
        }

        public void CreateDeck()
        {
            var vm = IoC.Get<ImportViewModel>();
            var result = windowManager.ShowDialogAsync(vm).Result;
            if (result == true)
                events.PublishOnUIThreadAsync(new CreateDeckEvent { Cards = vm.ImportedCards });
        }

        public void EditLocalData() 
            => events.PublishOnUIThreadAsync(new EditLocalDataEvent());

        public void OpenDeck()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Deck file (*.jd)|*.jd",
                InitialDirectory = Path.Combine( Constants.EXE_PATH, "decks" )
            };
            try
            {
                if (ofd.ShowDialog() != true)
                    return;

                events.PublishOnUIThreadAsync(new OpenDeckEvent(ofd.FileName));
            }
            catch (Exception e)
            {
                MessageBox.Show(Application.Current.MainWindow, e.Message);
            }
        }
    }
}
