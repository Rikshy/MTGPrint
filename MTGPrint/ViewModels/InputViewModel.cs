using Caliburn.Micro;

namespace MTGPrint.ViewModels
{
    public class InputViewModel : Screen
    {
        public string Text { get; set; }
        public string Input { get; set; }

        public void Confirm()
            => TryCloseAsync(true);
    }
}
