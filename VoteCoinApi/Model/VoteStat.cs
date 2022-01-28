using Newtonsoft.Json;

namespace VoteCoinApi.Model
{
    public class VoteStat
    {
        [JsonProperty("asa")]

        public ulong ASA { get; set; }

        [JsonProperty("events")]
        public ulong Events { get; set; }
        [JsonProperty("delegations")]
        public ulong Delegations { get; set; }
        [JsonProperty("questions")]
        public ulong Questions { get; set; }
    }
}
