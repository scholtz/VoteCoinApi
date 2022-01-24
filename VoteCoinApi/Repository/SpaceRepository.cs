using VoteCoinApi.Model;

namespace VoteCoinApi.Repository
{
    public class SpaceRepository
    {
        private readonly List<Space> spaces;
        public SpaceRepository()
        {
            this.spaces = new List<Space>() { };
            Init();
        }
        protected void Init()
        {
            var root = "../asa-list";
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
                    spaces.Add(new Space()
                    {
                        Asa = asa,
                        Unit = parts[0]
                    });
                }
            }
        }

        internal List<Space> List()
        {
            return spaces;
        }
    }
}
