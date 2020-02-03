using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading;
using System.Windows;
using System.Linq;
using System.IO;
using System;

using Microsoft.Win32;

using Caliburn.Micro;

using MTGPrint.EventModels;
using MTGPrint.Models;
using MTGPrint.Helper;

namespace MTGPrint.ViewModels
{
    class LocalDataViewModel : Screen
    {
        private readonly LocalDataStorage localData;
        private readonly IEventAggregator events;
        private readonly IWindowManager winMan;

        public LocalDataViewModel(SimpleContainer container)
        {
            localData = container.GetInstance<LocalDataStorage>();
            events = container.GetInstance<IEventAggregator>();
            winMan = container.GetInstance<IWindowManager>();

            searchCards = CollectionViewSource.GetDefaultView(localData.LocalCards);
            SearchText = string.Empty;
        }

        private string searchText;
        private ICollectionView searchCards;
        private LocalCard selectedItem;

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            localData.SaveLocalData();
            return await base.CanCloseAsync(cancellationToken);
        }

        public ICollectionView Cards
        {
            get => searchCards;
            set
            {
                searchCards = value;
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
                NotifyOfPropertyChange(() => CanDeleteCustomCard);
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                Cards.Filter = searchText.Length == 0 ? (Predicate<object>)null : ((o) => ((LocalCard)o).Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                events.PublishOnUIThreadAsync(new UpdateStatusEvent { Info = $"search found {localData.LocalCards.Count(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))} cards" });
                NotifyOfPropertyChange();
            }
        }

        public void OpenMainMenu()
           => events.PublishOnUIThreadAsync(new CloseScreenEvent());
        public void ForceRefresh()
        {
            events.PublishOnUIThreadAsync(new UpdateStatusEvent { Status = "Updating localdata", IsLoading = true, IsWndEnabled = false });
            localData.UpdateBulkData(true);
        }
        public void ShowInfo()
            => winMan.ShowDialogAsync(IoC.Get<InfoViewModel>()).Wait();

        public void AddCustomCard()
        {
            var vm = IoC.Get<InputViewModel>();
            vm.Text = "Enter the card name:";
            var result = winMan.ShowDialogAsync(vm).Result;
            if (result == true)
            {
                if (string.IsNullOrEmpty(vm.Input.Trim()))
                {
                    MessageBox.Show("Enter the card name!");
                    return;
                }

                if (localData.LocalCards.Any(c => c.Name.Equals(vm.Input, StringComparison.OrdinalIgnoreCase)))
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
                        Prints = new BindableCollection<CardPrint>(new[]{ cp }),
                    };

                    localData.LocalCards.Add(card);
                    SearchText = vm.Input;
                    SelectedItem = card;
                    localData.HasChanges = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void DeleteCustomCard()
        {
            if (MessageBox.Show("you sure?", "ORLY", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var temp = SelectedItem;
            localData.LocalCards.Remove(temp);
            Cards.Refresh();
            SearchText = "";
            localData.HasChanges = true;
            temp.Prints.ForEach(p => DeleteLocalFiles(p));
        }

        public void AddCustomPrint()
        {
            try
            {
                var print = CreateCardPrint();
                if (print == null)
                    return;

                SelectedItem.Prints.Insert(0, print);
                localData.HasChanges = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void RemoveCustomPrint(CardPrint o)
        {
            if (SelectedItem.Prints.Count == 1)
            {
                if (MessageBox.Show("This is the last print! It can only be delted with the card itself! continue?", "ORLY", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                DeleteCustomCard();
            }
            else
            {
                if (MessageBox.Show("you sure?", "ORLY", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                localData.HasChanges = true;
                SelectedItem.Prints.Remove(o);
                DeleteLocalFiles(o);
            }            
        }

        public bool CanAddCustomPrint
            => SelectedItem != null;

        public bool CanDeleteCustomCard
            => SelectedItem != null && SelectedItem.IsCustom;

        private void DeleteLocalFiles(CardPrint print)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        if (File.Exists(print.ImageUrls.Normal))
                            File.Delete(print.ImageUrls.Normal);
                        if (File.Exists(print.ImageUrls.BorderCrop))
                            File.Delete(print.ImageUrls.BorderCrop);
                    }
                    catch (Exception)
                    {
                    }

                    Thread.Sleep(100);
                }
            });
        }

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
