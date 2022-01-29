namespace VoteCoinApi.Model
{
    public class ApiConfig
    {
        public string AsaFolder { get; set; }
        public string Host { get; set; }
        public string MarketInfo { get; set; }
        public string TinyInfo { get; set; }
        public string StatsFile { get; set; }
        public Dictionary<string, ulong> TestnetMapping { get; set; }
    }
}
