using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.Windows;
using System.Linq;
using System;

using Microsoft.Win32;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    class LocalDataViewModel : Screen
    {
        private readonly SimpleContainer container;
        private readonly LocalDataStorage localData;
        private readonly IEventAggregator events;

        public LocalDataViewModel(SimpleContainer container)
        {
            localData = container.GetInstance<LocalDataStorage>();
            events = container.GetInstance<IEventAggregator>();

            searchCards = localData.LocalCards;
            this.container = container;
        }

        private string searchText;
        private IEnumerable<LocalCard> searchCards;
        private LocalCard selectedItem;

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            localData.SaveLocalData();
            return await base.CanCloseAsync(cancellationToken);
        }

        public IEnumerable<LocalCard> Cards
        {
            get => searchCards;
            set
            {
                searchCards = value;
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Info = $"search found {searchCards.Count()} cards" });
                NotifyOfPropertyChange();
            }
        }

        public LocalCard SelectedItem 
        { 
            get => selectedItem;
            set
            {
                selectedItem = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => CanAddCustomPrint);
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                Cards = searchText.Length == 0 ? 
                    localData.LocalCards : 
                    localData.LocalCards.Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                NotifyOfPropertyChange();
            }
        }

        public ICommand SearchChangedCommand { get; }

        public void OpenMainMenu()
           => events.PublishOnUIThreadAsync(new CloseScreenEvent());
        public void ShowInfo()
            => container.GetInstance<IWindowManager>().ShowDialogAsync(container.GetInstance<InfoViewModel>()).Wait();

        public void AddCustomCard()
        {
            var vm = container.GetInstance<InputViewModel>();
            vm.Text = "Enter the card name:";
            var result = container.GetInstance<IWindowManager>().ShowDialogAsync(vm).Result;
            if (result == true)
            {
                if (string.IsNullOrEmpty(vm.Input.Trim()))
                {
                    MessageBox.Show("Enter the card name!");
                    return;
                }

                if (Cards.Any(c => c.Name.Equals(vm.Input, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A card with this name already exists!");
                    return;
                }

                try
                {
                    var cp = CreateCardPrint();
                    if (cp == null)
                        return;

                    var card = new LocalCard
                    {
                        IsCustom = true,
                        OracleId = Guid.NewGuid(),
                        LatestPrint = DateTimeOffset.Now,
                        Name = vm.Input,
                        Prints = new List<CardPrint>(new[]{ cp }),
                    };

                    localData.LocalCards.Add(card);
                    SearchText = "";
                    Cards = localData.LocalCards;
                    SelectedItem = card;
                    localData.HasChanges = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void AddCustomPrint()
        {
            try
            {
                var print = CreateCardPrint();
                if (print == null)
                    return;

                SelectedItem.Prints.Add(print);
                localData.HasChanges = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public bool CanAddCustomPrint
            => SelectedItem != null;

        private CardPrint CreateCardPrint()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Card Picture (*.png)|*.png",
            };

            if (ofd.ShowDialog() != true)
                return null;

            return CardPrint.CreateCustom(ofd.FileName);
        }
    }
}
