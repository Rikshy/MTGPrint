namespace MTGPrint.Helper.Grabber
{
    public class TextGrabber : BaseGrabber
    {
        public override GrabMethod GrabMethod => GrabMethod.Text;

        public override bool IsMatching(string input) => true;

        protected override string GrabDeckList(string input)
            => input;
    }
}
