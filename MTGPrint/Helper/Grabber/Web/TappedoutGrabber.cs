namespace MTGPrint.Helper.Grabber.Web
{
    class TappedoutGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var url = importUrl;

            var idx = url.LastIndexOf('/');
            if (idx > 0 && !url.Substring(0, idx).EndsWith("mtg-decks"))
                url = url.Substring(0, idx);

            return $"{url}?fmt=txt";
        }

        public override bool IsMatching(string url)
        {
            return url.StartsWith("http://tappedout.net")
                || url.StartsWith("http://www.tappedout.net")
                || url.StartsWith("https://tappedout.net")
                || url.StartsWith("https://www.tappedout.net")
                || url.StartsWith("tappedout.net")
                || url.StartsWith("www.tappedout.net");
        }
    }
}
