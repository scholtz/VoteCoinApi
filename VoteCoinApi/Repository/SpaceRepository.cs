using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using VoteCoinApi.Model;

namespace VoteCoinApi.Repository
{
    public class SpaceRepository
    {
        private readonly IOptionsMonitor<Model.ApiConfig> config;

        private List<SpaceWithIcon> spaces;
        private List<ChartsItem> chartsInfo = new List<ChartsItem>();
        private Dictionary<ulong, TinyInfo> tinyInfo = new Dictionary<ulong, TinyInfo>();
        private Dictionary<ulong, VoteStat> statsInfo = new Dictionary<ulong, VoteStat>();
        private readonly ILogger<SpaceRepository> logger;
        public SpaceRepository(IOptionsMonitor<Model.ApiConfig> config, ILogger<SpaceRepository> logger)
        {
            this.logger = logger;
            this.config = config;
            this.spaces = new List<SpaceWithIcon>() { };
            Init();
        }
        protected void Init()
        {
            var root = config.CurrentValue.AsaFolder;
            DirectoryInfo dirInfo = new DirectoryInfo(root);
            var pngList = new HashSet<ulong>() { 230946361, 226701642, 233939122, 2751733, 142838028, 231880341 };
            var banList = new HashSet<ulong>() { 297995609 };
            var dirs = dirInfo.GetDirectories();
            if (!string.IsNullOrEmpty(config.CurrentValue.TinyInfo))
            {
                LoadTinyInfoFromFile();
            }
            foreach (var dir in dirs)
            {

                var parts = dir.Name.Split('-');
                if (parts.Length != 2) continue;
                if (ulong.TryParse(parts[1], out var asa))
                {
                    if (banList.Contains(asa))
                    {
                        continue;
                    }
                    var iconMime = "image/svg+xml";
                    var iconFile = "icon.svg";
                    if (pngList.Contains(asa))
                    {
                        iconFile = "icon.png";
                        iconMime = "image/png";
                    }

                    var iconpath = Path.Combine(root, dir.Name, iconFile);
                    if (!System.IO.File.Exists(iconpath)) continue;

                    var icon = System.IO.File.ReadAllBytes(iconpath);
                    tinyInfo.TryGetValue(asa, out var info);
                    var url = info?.URL;
                    if (!string.IsNullOrEmpty(url) && !(url.StartsWith("//") || url.StartsWith("http://") || url.StartsWith("https://")))
                    {
                        url = "https://" + url;
                    }
                    spaces.Add(new SpaceWithIcon()
                    {
                        Name = parts[0],
                        Asa = asa,
                        Unit = parts[0],
                        Icon = icon,
                        IconMimeType = iconMime,
                        Url = url,
                        IsVerified = info?.IsVerified ?? false,
                        Env = "mainnet"
                    });
                }
            }
            if (string.IsNullOrEmpty(config.CurrentValue.MarketInfo))
            {
                LoadMarketInfo().Wait();
            }
            if (chartsInfo.Count == 0)
            {
                LoadMarketInfoFromFile();
            }
            ProcessNamesFromTinyInfo();
            LoadStatsFromFile();

            MakeTestnet();

            SortSpaces();

        }

        private void MakeTestnet()
        {
            int i = 0;
            if (config.CurrentValue.TestnetMapping != null)
            {
                foreach (var mainnet in config.CurrentValue.TestnetMapping.Keys)
                {
                    if (ulong.TryParse(mainnet, out var asa))
                    {
                        var mainnetAsa = spaces.FirstOrDefault(s => s.Asa == asa && s.Env == "mainnet");
                        if (mainnetAsa != null)
                        {
                            var clone = mainnetAsa.ShallowCopy();
                            clone.Asa = config.CurrentValue.TestnetMapping[mainnet];
                            clone.Env = "testnet";
                            spaces.Add(clone);
                            i++;
                        }
                    }
                }
            }
            logger.LogInformation($"Testnet assets added: {config.CurrentValue?.TestnetMapping?.Count}/{i}");
        }

        private void LoadStatsFromFile()
        {
            try
            {
                var list = JsonConvert.DeserializeObject<List<VoteStat>>(File.ReadAllText(config.CurrentValue.StatsFile));
                logger.LogInformation($"Stats: loaded {list.Count} records");
                if (list?.Count > 0)
                {
                    statsInfo = list.ToDictionary(t => t.ASA, t => t);
                }
                foreach (var space in spaces)
                {
                    if (statsInfo.TryGetValue(space.Asa, out var info))
                    {
                        space.Events = info.Events;
                        space.Delegations = info.Delegations;
                        space.Questions = info.Questions;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Unable to process stats file: {ex.Message}");
            }
        }
        private void ProcessNamesFromTinyInfo()
        {
            foreach (var space in spaces)
            {
                if (tinyInfo.TryGetValue(space.Asa, out var info))
                {
                    space.Name = info.Name;
                }
            }
        }
        public async Task LoadMarketInfo()
        {
            var client = new RestClient("https://tinychart.org");
            var request = new RestSharp.RestRequest("/api/pools?p=T2", RestSharp.Method.Get);
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var list = JsonConvert.DeserializeObject<List<ChartsItem>>(response.Content ?? "[]");
                if (list?.Count > 100)
                {
                    chartsInfo = list;
                }
            }
        }
        public void LoadMarketInfoFromFile()
        {
            var list = JsonConvert.DeserializeObject<List<ChartsItem>>(File.ReadAllText(config.CurrentValue.MarketInfo));
            if (list?.Count > 100)
            {
                chartsInfo = list;
            }
        }
        public void LoadTinyInfoFromFile()
        {
            var list = JsonConvert.DeserializeObject<TinyInfoCover>(File.ReadAllText(config.CurrentValue.TinyInfo));
            if (list?.Results?.Count > 100)
            {
                tinyInfo = list.Results.ToDictionary(i => i.Id, i => i);
            }
        }
        public void SortSpaces()
        {
            foreach (var space in spaces)
            {
                var sumOfLiquidity = chartsInfo.Where(c => c.Asset1Id == space.Asa || c.Asset1Id == space.Asa).Sum(c => c.Liquidity);
                space.Order = 10000 * space.Events + sumOfLiquidity;
            }
            spaces = spaces.OrderByDescending(c => c.Order).ToList();
        }
        internal IEnumerable<SpaceBase> List(string env)
        {
            var host = config.CurrentValue.Host.TrimEnd('/');
            return spaces.Where(o => o.Env == env).Select(o =>
              {
                  var space = o as SpaceBase;
                  if (o.IconMimeType == "image/png")
                  {
                      space.IconPath = $"{host}/Space/{o.Asa}/Icon.png";
                  }
                  else
                  {
                      space.IconPath = $"{host}/Space/{o.Asa}/Icon.svg";
                  }
                  return space;
              }
            );
        }
        internal byte[]? Icon(string env, ulong asa)
        {
            return spaces.FirstOrDefault(a => a.Asa == asa && env == a.Env)?.Icon;
        }
        internal string? IconMime(string env, ulong asa)
        {
            return spaces.FirstOrDefault(a => a.Asa == asa && env == a.Env)?.IconMimeType;
        }
    }
}
