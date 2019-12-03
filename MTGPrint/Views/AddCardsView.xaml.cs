using MTGPrint.Helper.UI;
using System.Windows;

namespace MTGPrint.Views
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
    }
}
