using System.Text.RegularExpressions;

namespace MTGPrint.Helper.Grabber.Web
{
    class DeckstatsGrabber : BaseWebGrabber
    {
        protected override string RefineUrl(string importUrl)
            => importUrl.Contains('?') ? $"{importUrl}&export_dec=1" : $"{importUrl}?export_dec=1";

        public override bool IsMatching(string url)
            => url.StartsWith("https://deckstats.net") || url.StartsWith("https://www.deckstats.net");

        protected override string RefineResponse(string reponse)
            => Regex.Replace(reponse, @"\[.*\] ", string.Empty, RegexOptions.None);
    }
}
