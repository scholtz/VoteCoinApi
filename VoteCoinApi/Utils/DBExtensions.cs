using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoteCoinMonitor.Model;

namespace VoteCoinMonitor.Utils
{
    public class DBExtensions
    {
        public static void StoreDB(DB db, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(db));
        }
        public static DB LoadDB(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            return JsonConvert.DeserializeObject<DB>(File.ReadAllText(path));
        }
    }
}
