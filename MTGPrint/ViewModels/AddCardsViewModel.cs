using Caliburn.Micro;

namespace MTGPrint.ViewModels
{
    public class AddCardsViewModel : Screen
    {
        public string ImportCards { get; set; }

        public void AddCards()
            => TryCloseAsync(true);
    }
}
