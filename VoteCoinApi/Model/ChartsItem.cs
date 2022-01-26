using Newtonsoft.Json;

namespace VoteCoinApi.Model
{
    public class ChartsItem
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("asset_1_id")]
        public ulong? Asset1Id { get; set; }
        [JsonProperty("asset_2_id")]
        public ulong? Asset2Id { get; set; }
        [JsonProperty("created_round")]
        public ulong CreatedRound { get; set; }
        [JsonProperty("id")]
        public ulong Id { get; set; }
        [JsonProperty("liquidity")]
        public decimal Liquidity { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
        [JsonProperty("price24h")]
        public decimal Price24H { get; set; }
        [JsonProperty("volatility")]
        public decimal Volatility { get; set; }

    }
}
