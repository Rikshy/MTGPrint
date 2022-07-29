using System.Collections.Generic;

using MTGPrint.Models;

namespace MTGPrint.EventModels
{
    public class CreateDeckEvent
    {
        public IEnumerable<DeckCard> Cards { get; set; }
    }
}
