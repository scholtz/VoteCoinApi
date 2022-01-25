using Microsoft.Extensions.Options;
using VoteCoinApi.Model;

namespace VoteCoinApi.Repository
{
    public class SpaceRepository
    {
        private readonly IOptionsMonitor<Model.ApiConfig> config;

        private readonly List<SpaceWithIcon> spaces;
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
            foreach (var dir in dirs)
            {
                var iconpath = Path.Combine(root, dir.Name, "icon.svg");
                var parts = dir.Name.Split('-');
                if (parts.Length != 2) continue;
                if (!System.IO.File.Exists(iconpath)) continue;
                if (ulong.TryParse(parts[1], out var asa))
                {
                    var icon = System.IO.File.ReadAllBytes(iconpath);
                    spaces.Add(new SpaceWithIcon()
                    {
                        Asa = asa,
                        Unit = parts[0],
                        Icon = icon
                    });
                }
            }
        }

        internal IEnumerable<SpaceBase> List()
        {
            return spaces.Select(o => o as SpaceBase);
        }
        internal byte[] Icon(ulong asa)
        {
            return spaces.FirstOrDefault(a => a.Asa == asa)?.Icon;
        }
    }
}
