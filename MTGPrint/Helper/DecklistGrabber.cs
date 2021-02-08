using System.Collections.Generic;
using System.Linq;
using System;

using MTGPrint.Models;
using MTGPrint.Helper.Grabber;
using MTGPrint.Helper.Grabber.Web;

namespace MTGPrint.Helper
{
    public class DecklistGrabber
    {
        private static readonly List<BaseGrabber> grabbers = new List<BaseGrabber>
        {
            new TextGrabber(),
            new DeckstatsGrabber(),
            new ScryfallGrabber(),
            new GoldfishGrabber(),
            new AetherhubGrabber(),
            new TappedoutGrabber(),
            new ArchidektGrabber()
        };

        public static IEnumerable<DeckCard> Grab(string source, GrabMethod method, out IEnumerable<string> errors)
        {
            var grabber = grabbers.FirstOrDefault(g => g.GrabMethod == method && g.IsMatching(source));

            if (grabber == null)
                throw new ApplicationException("No decklistgrabber found for your input.");

            return grabber.Grab(source, out errors);
        }
    }

    public enum GrabMethod
    {
        Text = 0,
        Url
    }
}
