using System.Windows;

namespace MTGPrint.Views
{
    public partial class PrintView : Window
    {
        public PrintView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
