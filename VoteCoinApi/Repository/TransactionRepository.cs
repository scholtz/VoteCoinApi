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
        private VoteCoinMonitor.Model.DB? CacheTestnet = null;
        private DateTimeOffset? CacheUpdated = null;
        private DateTimeOffset? CacheTestnetUpdated = null;
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
                if (File.Exists(config.CurrentValue.TransactionsDBFile))
                {
                    logger.LogInformation("Updating cache");
                    Cache = VoteCoinMonitor.Utils.DBExtensions.LoadDB(config.CurrentValue.TransactionsDBFile);
                    CacheUpdated = DateTimeOffset.Now;
                    logger.LogInformation($"Cache: {string.Join(",", Cache.LatestAssetCheckedInBlock.Select(k => $"{k.Key}={k.Value}"))}");

                    foreach (var item in Cache.LatestAssetCheckedInBlock)
                    {
                        try
                        {
                            var tlTxs = 0;
                            var voteTxs = 0;
                            var questionTxs = 0;
                            var delegationTxs = 0;
                            if (Cache.AssetsTrustedListTxs.TryGetValue(item.Key, out var list))
                            {
                                tlTxs = list.Count;
                            }
                            if (Cache.AssetsVoteTxs.TryGetValue(item.Key, out var listAssetsVoteTxs))
                            {
                                voteTxs = listAssetsVoteTxs.Count;
                            }
                            if (Cache.AssetsQuestionTxs.TryGetValue(item.Key, out var listAssetsQuestionTxs))
                            {
                                questionTxs = listAssetsQuestionTxs.Count;
                            }
                            if (Cache.AssetsDelegationTxs.TryGetValue(item.Key, out var listAssetsDelegationTxs))
                            {
                                delegationTxs = listAssetsDelegationTxs.Count;
                            }
                            var events = tlTxs +
                                            voteTxs +
                                            questionTxs +
                                            delegationTxs;
                            logger.LogInformation($"Updating cache: {item.Key} {events}");
                            spaceRepository.UpdateStats(item.Key, events, delegations: delegationTxs, questions: questionTxs, "mainnet");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.Message, ex);
                        }
                    }
                }
                else
                {
                    logger.LogInformation("Mainnet db does not exists");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
            }

            try
            {
                if (File.Exists(config.CurrentValue.TransactionsTestnetDBFile))
                {
                    logger.LogInformation("Updating testnet cache");
                    CacheTestnet = VoteCoinMonitor.Utils.DBExtensions.LoadDB(config.CurrentValue.TransactionsDBFile);
                    CacheTestnetUpdated = DateTimeOffset.Now;
                    logger.LogInformation($"Cache: {string.Join(",", CacheTestnet.LatestAssetCheckedInBlock.Select(k => $"{k.Key}={k.Value}"))}");

                    foreach (var item in CacheTestnet.LatestAssetCheckedInBlock)
                    {
                        try
                        {
                            var tlTxs = 0;
                            var voteTxs = 0;
                            var questionTxs = 0;
                            var delegationTxs = 0;
                            if (CacheTestnet.AssetsTrustedListTxs.TryGetValue(item.Key, out var list))
                            {
                                tlTxs = list.Count;
                            }
                            if (CacheTestnet.AssetsVoteTxs.TryGetValue(item.Key, out var listAssetsVoteTxs))
                            {
                                voteTxs = listAssetsVoteTxs.Count;
                            }
                            if (CacheTestnet.AssetsQuestionTxs.TryGetValue(item.Key, out var listAssetsQuestionTxs))
                            {
                                questionTxs = listAssetsQuestionTxs.Count;
                            }
                            if (CacheTestnet.AssetsDelegationTxs.TryGetValue(item.Key, out var listAssetsDelegationTxs))
                            {
                                delegationTxs = listAssetsDelegationTxs.Count;
                            }
                            var events = tlTxs +
                                            voteTxs +
                                            questionTxs +
                                            delegationTxs;
                            logger.LogInformation($"Updating testnet cache: {item.Key} {events}");
                            spaceRepository.UpdateStats(item.Key, events, delegations: delegationTxs, questions: questionTxs, "testnet");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.Message, ex);
                        }
                    }
                }
                else
                {
                    logger.LogInformation("Testnet db does not exists");
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
