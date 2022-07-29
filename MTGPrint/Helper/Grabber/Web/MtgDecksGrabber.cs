namespace MTGPrint.Helper.Grabber.Web
{
    internal class MtgDecksGrabber : BaseWebGrabber
    {
        public override bool IsMatching(string input)
            => input.StartsWith("https://mtgdecks.net") || input.StartsWith("https://www.mtgdecks.net");

        protected override string RefineUrl(string importUrl) 
            => $"{importUrl}/txt";
    }
}
