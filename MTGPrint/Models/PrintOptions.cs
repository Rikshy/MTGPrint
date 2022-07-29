using Newtonsoft.Json;

namespace MTGPrint.Models
{
    public class PrintOptions
    {
        [JsonProperty("card_border")]
        public CardBorder CardBorder { get; set; } = CardBorder.With;

        [JsonProperty("card_margin")]
        public double CardMargin { get; set; } = 1;

        [JsonProperty("card_scaling")]
        public double CardScaling { get; set; } = 100;

        [JsonProperty("open_pdf")]
        public bool OpenPDF { get; set; } = true;
    }
}
