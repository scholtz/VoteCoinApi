using Algorand.V2.Indexer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoteCoinMonitor.Model
{
    public class DB
    {
        public ConcurrentDictionary<ulong, ulong> LatestAssetCheckedInBlock { get; set; } = new ConcurrentDictionary<ulong, ulong>();
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>> AssetsDelegationTxs { get; set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>>();
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>> AssetsQuestionTxs { get; set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>>();
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>> AssetsVoteTxs { get; set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>>();
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>> AssetsTrustedListTxs { get; set; } = new ConcurrentDictionary<ulong, ConcurrentDictionary<string, Transaction>>();
    }
}
