﻿using MTGPrint.Helper;
using System.Windows;

namespace MTGPrint
{
    public partial class AddCardsView : Window
    {
        public AddCardsView()
        {
            InitializeComponent();
            SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
