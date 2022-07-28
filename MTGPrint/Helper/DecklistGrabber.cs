using System.Collections.Generic;
using System.Linq;
using System;

using MTGPrint.Models;
using MTGPrint.Helper.Grabber;
using System.Reflection;

namespace MTGPrint.Helper
{
    public class DecklistGrabber
    {
        private static readonly List<BaseGrabber> grabbers = new();

        static DecklistGrabber()
        {
            LoadGrabber(typeof(BaseGrabber));
        }

        private static void LoadGrabber(Type baseType)
        {
            var grabs = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == baseType);
            foreach (var grab in grabs)
            {
                if (grab.IsAbstract)
                    LoadGrabber(grab);
                else
                    grabbers.Add((BaseGrabber)Activator.CreateInstance(grab));
            }
        }

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
