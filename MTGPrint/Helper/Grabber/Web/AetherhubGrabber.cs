using System;
using System.Text.RegularExpressions;

namespace MTGPrint.Helper.Grabber.Web
{
    class AetherhubGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var url = importUrl;

            var idx = url.LastIndexOf('/');
            var deckName = url.Substring(idx + 1);
            var idMatch = Regex.Match(deckName, "([0-9]+)");
            if (idMatch.Success && idMatch.Groups[1].Success)
                return $"https://aetherhub.com/Deck/MtgoDeckExport/{idMatch.Groups[1].Value}";

            throw new ApplicationException("deckid not found");
        }

        public override bool IsMatching(string url)
            => url.StartsWith("https://aetherhub.com") || url.StartsWith("https://www.aetherhub.com");
    }
}
