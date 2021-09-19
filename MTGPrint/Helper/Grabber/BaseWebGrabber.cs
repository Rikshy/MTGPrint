namespace MTGPrint.Helper.Grabber
{
    public abstract class BaseWebGrabber : BaseGrabber
    {
        public override GrabMethod GrabMethod => GrabMethod.Url;

        protected abstract string RefineUrl(string importUrl);
        protected override string GrabDeckList(string importUrl)
        {
            var url = RefineUrl(importUrl);

            var responseText = WebHelper.Get(url);

            return RefineResponse(responseText);
        }

        protected virtual string RefineResponse(string response)
            => response;
    }
}
