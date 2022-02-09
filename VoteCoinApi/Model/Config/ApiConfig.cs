namespace VoteCoinApi.Model.Config
{
    public class ApiConfig
    {
        public string AsaFolder { get; set; }
        public string Host { get; set; }
        public string MarketInfo { get; set; }
        public string TinyInfo { get; set; }
        public string StatsFile { get; set; }
        public Dictionary<string, ulong> TestnetMapping { get; set; }
        public string TransactionsDBFile { get; set; } = "/app/data/db.json";
        public string TransactionsTestnetDBFile { get; set; } = "/app/data-testnet/testnet.json";
    }
}
