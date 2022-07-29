using System;

namespace MTGPrint.Helper.Grabber.Web
{
    internal class MoxfieldGrabber : BaseWebGrabber
    {
        public override bool IsMatching(string input)
            => input.StartsWith("https://moxfield.com") || input.StartsWith("https://www.moxfield.com");

        protected override string RefineUrl(string importUrl)
        {
            var deckId = importUrl.Substring(importUrl.LastIndexOf('/') + 1);
            return $"https://api.moxfield.com/v1/decks/all/{deckId}/download?exportId=09469bfd-90b7-433b-80d2-dbdae4c4dc06&arenaOnly=false";
        }
    }
}
