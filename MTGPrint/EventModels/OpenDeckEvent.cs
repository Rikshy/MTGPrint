namespace MTGPrint.EventModels
{
    public class OpenDeckEvent
    {
        public OpenDeckEvent(string path)
            => DeckPath = path;

        public string DeckPath { get; }
    }
}
