namespace MTGPrint.Helper.Grabber.Web
{
    class GoldfishGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var url = importUrl;

            var idx = url.LastIndexOf('#');
            if (idx > 0)
                url = url.Substring(0, idx);

            idx = url.LastIndexOf('/');
            var deckId = url.Substring(idx + 1);
            return $"https://mtggoldfish.com/deck/download/{deckId}";
        }

        public override bool IsMatching(string url)
            => url.StartsWith("https://mtggoldfish.com") || url.StartsWith("https://www.mtggoldfish.com");
    }
}
