namespace MTGPrint.Helper.Grabber.Web
{
    class AetherhubGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var url = importUrl;

            var idx = url.LastIndexOf('/');
            var deckId = url.Substring(idx + 1);
            return $"https://aetherhub.com/Deck/MtgoDeckExport/{deckId}";
        }

        public override bool IsMatching(string url)
            => url.StartsWith("https://aetherhub.com") || url.StartsWith("https://www.aetherhub.com");
    }
}
