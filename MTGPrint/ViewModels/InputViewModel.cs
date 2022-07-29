using Caliburn.Micro;

namespace MTGPrint.ViewModels
{
    public class InputViewModel : Screen
    {
        private string input = string.Empty;

        public string Text { get; set; }
        public string Input
        {
            get => input;
            set
            {
                input = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanConfirm));
            }
        }

        public bool CanConfirm => !string.IsNullOrEmpty(Input.Trim());

        public void Confirm()
            => TryCloseAsync(true);
    }
}
