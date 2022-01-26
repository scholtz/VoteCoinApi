using Newtonsoft.Json;

namespace VoteCoinApi.Model
{
    public class TinyInfo
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("is_liquidity_token")]
        public bool IsLiquidityToken { get; set; }
        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("unit_name")]
        public string UnitName { get; set; }
        [JsonProperty("id")]
        public ulong Id { get; set; }
        [JsonProperty("decimals")]
        public int Decimals { get; set; }
        [JsonProperty("total_amount")]
        public ulong? TotalAmount { get; set; }
        [JsonProperty("url")]
        public string URL { get; set; }

    }
}
