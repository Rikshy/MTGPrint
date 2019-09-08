using System.Windows;

namespace MTGPrint
{
    /// <summary>
    /// Interaktionslogik für AddCards.xaml
    /// </summary>
    public partial class AddCardsView : Window
    {
        public AddCardsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
