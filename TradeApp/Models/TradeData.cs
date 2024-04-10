using Newtonsoft.Json;

namespace TradeApp.Models
{
    public class TradeData
    {
        [JsonProperty("s")]
        public string Symbol { get; set; }

        [JsonProperty("p")]
        public decimal Price { get; set; }

        [JsonProperty("q")]
        public decimal Quantity { get; set; }

        [JsonProperty("m")]
        public bool IsBuyer { get; set; }

        [JsonProperty("T")]
        public long TimeStamp { get; set; }
        public DateTime Time => DateTimeOffset.FromUnixTimeMilliseconds(TimeStamp).UtcDateTime;
    }


}
