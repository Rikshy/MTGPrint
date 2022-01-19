using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace MTGPrint.Helper.Grabber.Web
{
    class GoldfishGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
        {
            var pageContant = new HttpClient().GetStringAsync(importUrl).Result;

            var idMatch = Regex.Match(pageContant, "\"\\/deck\\/download\\/(.*)\"");

            if (idMatch.Success && idMatch.Groups[1].Success)
                return $"https://mtggoldfish.com/deck/download/{idMatch.Groups[1].Value}";

            throw new ApplicationException("deckid not found");
        }

        public override bool IsMatching(string url)
            => url.StartsWith("https://mtggoldfish.com") || url.StartsWith("https://www.mtggoldfish.com");
    }
}
