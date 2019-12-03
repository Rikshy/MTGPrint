using System.Windows;

namespace MTGPrint.Views
{
    public partial class InfoView : Window
    {
        public InfoView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start( e.Uri.AbsoluteUri );
        }
    }
}
