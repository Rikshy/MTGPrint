using Caliburn.Micro;
using Microsoft.Win32;
using MTGPrint.EventModels;
using MTGPrint.Helper;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MTGPrint.ViewModels
{
    public class MainMenuViewModel : Screen
    {
        private readonly IEventAggregator events;

        public MainMenuViewModel(IEventAggregator events)
        {
            this.events = events;
            OpenDeckCommand = new LightCommand(OpenDeck);
            EditLocalDataCommand = new LightCommand(() => events.PublishOnUIThreadAsync(new EditLocalDataEvent()));

            NewDeckCommand = new LightCommand(() => events.PublishOnUIThreadAsync(new CreateDeckEvent()));
        }

        public ICommand OpenDeckCommand { get; }
        public ICommand NewDeckCommand { get; }
        public ICommand EditLocalDataCommand { get; }

        private void OpenDeck()
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
