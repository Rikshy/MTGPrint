using System.Windows;
using System.IO;
using System;

using Microsoft.Win32;

using Caliburn.Micro;

using MTGPrint.EventModels;

namespace MTGPrint.ViewModels
{
    public class MainMenuViewModel : Screen
    {
        private readonly IEventAggregator events;
        private readonly IWindowManager winMan;

        public MainMenuViewModel(SimpleContainer container)
        {
            events = container.GetInstance<IEventAggregator>();
            winMan = container.GetInstance<IWindowManager>();
        }

        public void CreateDeck()
        {
            var vm = IoC.Get<ImportViewModel>();
            var result = winMan.ShowDialogAsync(vm).Result;
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
                InitialDirectory = Path.Combine(Environment.CurrentDirectory, "decks")
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
