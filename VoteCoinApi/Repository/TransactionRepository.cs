using Algorand.V2.Indexer.Model;
using Microsoft.Extensions.Options;

namespace VoteCoinApi.Repository
{
    public class TransactionRepository
    {
        private readonly IOptionsMonitor<Model.Config.ApiConfig> config;
        private readonly ILogger<SpaceRepository> logger;
        private readonly SpaceRepository spaceRepository;
        private VoteCoinMonitor.Model.DB? Cache = null;
        private DateTimeOffset? CacheUpdated = null;
        public TransactionRepository(SpaceRepository spaceRepository, IOptionsMonitor<Model.Config.ApiConfig> config, ILogger<SpaceRepository> logger)
        {
            this.config = config;
            this.logger = logger;
            this.spaceRepository = spaceRepository;
            UpdateCache();
        }
        public void UpdateCache()
        {
            if (CacheUpdated != null && CacheUpdated.Value.AddSeconds(60) > DateTimeOffset.Now)
            {
                return;
            }
            try
            {
                logger.LogInformation("Updating cache");
                Cache = VoteCoinMonitor.Utils.DBExtensions.LoadDB(config.CurrentValue.TransactionsDBFile);
                CacheUpdated = DateTimeOffset.Now;
                logger.LogInformation($"Cache: {string.Join(",", Cache.LatestAssetCheckedInBlock.Select(k => $"{k.Key}={k.Value}"))}");

                foreach (var item in Cache.LatestAssetCheckedInBlock)
                {
                    try
                    {
                        var events = Cache.AssetsTrustedListTxs[item.Key].Count +
                                        Cache.AssetsVoteTxs[item.Key].Count +
                                        Cache.AssetsQuestionTxs[item.Key].Count +
                                        Cache.AssetsDelegationTxs[item.Key].Count;

                        spaceRepository.UpdateStats(item.Key, events, delegations: Cache.AssetsDelegationTxs[item.Key].Count, questions: Cache.AssetsQuestionTxs[item.Key].Count);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message, ex);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }
        }
        internal IEnumerable<Transaction> ListDelegations(string env, ulong assetId)
        {
            UpdateCache();

            if (Cache != null && Cache.AssetsDelegationTxs.TryGetValue(assetId, out var collection))
            {
                return collection.Values;
            }
            return Enumerable.Empty<Transaction>();
        }
        internal IEnumerable<Transaction> ListQuestions(string env, ulong assetId)
        {
            UpdateCache();

            if (Cache != null && Cache.AssetsQuestionTxs.TryGetValue(assetId, out var collection))
            {
                return collection.Values;
            }
            return Enumerable.Empty<Transaction>();
        }
        internal IEnumerable<Transaction> AssetsTrustedListTxs(string env, ulong assetId)
        {
            UpdateCache();

            if (Cache != null && Cache.AssetsTrustedListTxs.TryGetValue(assetId, out var collection))
            {
                return collection.Values;
            }
            return Enumerable.Empty<Transaction>();
        }
        internal IEnumerable<Transaction> AssetsVoteTxs(string env, ulong assetId)
        {
            UpdateCache();

            if (Cache != null && Cache.AssetsVoteTxs.TryGetValue(assetId, out var collection))
            {
                return collection.Values;
            }
            return Enumerable.Empty<Transaction>();
        }
    }
}
