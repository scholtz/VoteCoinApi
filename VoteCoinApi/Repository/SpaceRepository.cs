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
        public SpaceRepository(IOptionsMonitor<Model.ApiConfig> config)
        {
            this.config = config;
            this.spaces = new List<SpaceWithIcon>() { };
            Init();
        }
        protected void Init()
        {
            var root = config.CurrentValue.AsaFolder;
            DirectoryInfo dirInfo = new DirectoryInfo(root);
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
                    var iconMime = "image/svg+xml";
                    var iconFile = "icon.svg";
                    if (asa == 226701642)
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
                        Asa = asa,
                        Unit = parts[0],
                        Icon = icon,
                        IconMimeType = iconMime,
                        Url = url,
                        IsVerified = info?.IsVerified ?? false
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
            SortSpaces();
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
                space.Order = sumOfLiquidity;
            }
            spaces = spaces.OrderByDescending(c => c.Order).ToList();
        }
        internal IEnumerable<SpaceBase> List()
        {
            var host = config.CurrentValue.Host.TrimEnd('/');
            return spaces.Select(o =>
            {
                var space = o as SpaceBase;
                space.IconPath = $"{host}/Space/{o.Asa}/Icon.svg";
                return space;
            }
            );
        }
        internal byte[]? Icon(ulong asa)
        {
            return spaces.FirstOrDefault(a => a.Asa == asa)?.Icon;
        }
        internal string? IconMime(ulong asa)
        {
            return spaces.FirstOrDefault(a => a.Asa == asa)?.IconMimeType;
        }
    }
}
