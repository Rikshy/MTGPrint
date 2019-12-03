using System.Diagnostics;

namespace MTGPrint.ViewModels
{
    public class InfoViewModel
    {
        public void OpenProjectUrl()
            => Process.Start(new ProcessStartInfo("https://github.com/Rikshy/MTGPrint") { UseShellExecute = true });
    }
}
