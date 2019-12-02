using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using System;

using Microsoft.Win32;

using MTGPrint.Windows;
using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    class LocalDbViewModel : INotifyPropertyChanged
    {
        public LocalDbViewModel()
        {
            var cards = LocalDataStorage.LocalCards;
            searchCards = cards;
            WindowClosedCommand = new LightCommand(() => LocalDataStorage.SaveLocalData());
            SearchChangedCommand = new EventCommand<TextChangedEventArgs>((e) =>
            {
                var txt = ((TextBox)e.Source).Text.Trim();
                Cards = txt.Length == 0 ? cards : cards.Where(c => c.Name.Contains(txt, StringComparison.OrdinalIgnoreCase));
            });
            AddCustomCardCommand = new LightCommand(AddCustomCard);
            AddCustomPrintCommand = new LightCommand(AddCustomArt);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string searchText;
        private IEnumerable<LocalCard> searchCards;
        private LocalCard selectedItem;

        public IEnumerable<LocalCard> Cards
        {
            get => searchCards;
            set
            {
                searchCards = value;
                OnPropertyChanged();
            }
        }

        public LocalCard SelectedItem 
        { 
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged();
            }
        }

        public ICommand WindowClosedCommand { get; }

        public ICommand SearchChangedCommand { get; }

        public ICommand AddCustomCardCommand { get; }
        public ICommand AddCustomPrintCommand { get; }

        private void AddCustomCard()
        {
            var vm = new InputViewModel { Text = "Enter the card name:" };
            var input = new InputWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (input.ShowDialog() == true)
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
                        Prints = new ObservableCollection<CardPrint>(new[]{ cp }),
                    };

                    LocalDataStorage.LocalCards.Add(card);
                    SearchText = "";
                    Cards = LocalDataStorage.LocalCards;
                    SelectedItem = card;
                    LocalDataStorage.HasChanges = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private void AddCustomArt()
        {
            try
            {
                var print = CreateCardPrint();
                if (print == null)
                    return;

                SelectedItem.Prints.Add(print);
                LocalDataStorage.HasChanges = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
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
