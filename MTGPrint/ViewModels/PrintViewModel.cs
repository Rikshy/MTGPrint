using Caliburn.Micro;

using MTGPrint.Models;

namespace MTGPrint.ViewModels
{
    public class PrintViewModel : Screen
    {        
        public PrintOptions PrintOptions { get; set; }

        public void Print()
            => TryCloseAsync(true);
    }
}
