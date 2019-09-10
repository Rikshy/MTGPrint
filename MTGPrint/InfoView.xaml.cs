using System.Windows;

namespace MTGPrint
{
    /// <summary>
    /// Interaktionslogik für InfoView.xaml
    /// </summary>
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
