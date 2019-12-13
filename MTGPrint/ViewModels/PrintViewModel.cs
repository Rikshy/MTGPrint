using Caliburn.Micro;

using MTGPrint.Helper;
using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class PrintViewModel : Screen
    {
        public PrintViewModel(BackgroundPrinter printer)
            => PrintOptions = printer.LoadPrintSettings();
        
        public PrintOptions PrintOptions { get; set; }

        public void Print()
            => TryCloseAsync(true);
    }
}
