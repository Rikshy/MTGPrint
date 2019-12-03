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
        private readonly IEventAggregator events;

        public MainMenuViewModel(IEventAggregator events) 
            => this.events = events;

        public void CreateDeck() 
            => events.PublishOnUIThreadAsync(new CreateDeckEvent());

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
