using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Applications.Radarr
{
    public class Radarr : ApplicationBase<RadarrSettings>
    {
        public override string Name => "Radarr";

        private readonly IRadarrV3Proxy _radarrV3Proxy;
        private readonly ICached<List<RadarrIndexer>> _schemaCache;
        private readonly IConfigFileProvider _configFileProvider;

        public Radarr(ICacheManager cacheManager, IRadarrV3Proxy radarrV3Proxy, IConfigFileProvider configFileProvider, IAppIndexerMapService appIndexerMapService, Logger logger)
            : base(appIndexerMapService, logger)
        {
            _schemaCache = cacheManager.GetCache<List<RadarrIndexer>>(GetType());
            _radarrV3Proxy = radarrV3Proxy;
            _configFileProvider = configFileProvider;
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_radarrV3Proxy.Test(Settings));

            return new ValidationResult(failures);
        }

        public override void AddIndexer(IndexerDefinition indexer)
        {
            if (indexer.Capabilities.Categories.SupportedCategories(Settings.SyncCategories.ToArray()).Any())
            {
                var schema = _schemaCache.Get(Definition.Settings.ToJson(), () => _radarrV3Proxy.GetIndexerSchema(Settings), TimeSpan.FromDays(7));
                var newznab = schema.Where(i => i.Implementation == "Newznab").First();
                var torznab = schema.Where(i => i.Implementation == "Torznab").First();

                var radarrIndexer = BuildRadarrIndexer(indexer, indexer.Protocol == DownloadProtocol.Usenet ? newznab : torznab);

                var remoteIndexer = _radarrV3Proxy.AddIndexer(radarrIndexer, Settings);
                _appIndexerMapService.Insert(new AppIndexerMap { AppId = Definition.Id, IndexerId = indexer.Id, RemoteIndexerId = remoteIndexer.Id });
            }
        }

        public override void RemoveIndexer(int indexerId)
        {
            var appMappings = _appIndexerMapService.GetMappingsForApp(Definition.Id);

            var indexerMapping = appMappings.FirstOrDefault(m => m.IndexerId == indexerId);

            if (indexerMapping != null)
            {
                //Remove Indexer remotely and then remove the mapping
                _radarrV3Proxy.RemoveIndexer(indexerMapping.RemoteIndexerId, Settings);
                _appIndexerMapService.Delete(indexerMapping.Id);
            }
        }

        public override void UpdateIndexer(IndexerDefinition indexer)
        {
            //Use the Id mapping here to delete the correct indexer
            throw new System.NotImplementedException();
        }

        private RadarrIndexer BuildRadarrIndexer(IndexerDefinition indexer, RadarrIndexer schema)
        {
            var radarrIndexer = new RadarrIndexer
            {
                Id = 0,
                Name = $"{indexer.Name} (Prowlarr)",
                EnableRss = true,
                EnableAutomaticSearch = true,
                EnableInteractiveSearch = true,
                Priority = indexer.Priority,
                Implementation = indexer.Protocol == DownloadProtocol.Usenet ? "Newznab" : "Torznab",
                ConfigContract = schema.ConfigContract,
                Fields = schema.Fields,
            };

            radarrIndexer.Fields.FirstOrDefault(x => x.Name == "baseUrl").Value = $"{Settings.ProwlarrUrl}/api/v1/indexer/{indexer.Id}/";
            radarrIndexer.Fields.FirstOrDefault(x => x.Name == "apiPath").Value = "/newznab";
            radarrIndexer.Fields.FirstOrDefault(x => x.Name == "apiKey").Value = _configFileProvider.ApiKey;

            return radarrIndexer;
        }
    }
}