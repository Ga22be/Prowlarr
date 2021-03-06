using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcorn : HttpIndexerBase<PassThePopcornSettings>
    {
        public override string Name => "PassThePopcorn";
        public override string BaseUrl => "https://passthepopcorn.me";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override IndexerPrivacy Privacy => IndexerPrivacy.Private;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;

        public override IndexerCapabilities Capabilities => SetCapabilities();

        public override int PageSize => 50;

        public PassThePopcorn(IHttpClient httpClient,
            ICacheManager cacheManager,
            IIndexerStatusService indexerStatusService,
            IConfigService configService,
            Logger logger)
            : base(httpClient, indexerStatusService, configService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new PassThePopcornRequestGenerator()
            {
                Settings = Settings,
                HttpClient = _httpClient,
                Logger = _logger,
                BaseUrl = BaseUrl
            };
        }

        private IndexerCapabilities SetCapabilities()
        {
            var caps = new IndexerCapabilities
            {
                SearchParams = new List<SearchParam>
                {
                    SearchParam.Q
                },
                MovieSearchParams = new List<MovieSearchParam>
                {
                    MovieSearchParam.Q, MovieSearchParam.ImdbId
                }
            };

            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.Movies, "Feature Film");
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesForeign);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesOther);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesSD);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesHD);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.Movies3D);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesBluRay);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesDVD);
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.MoviesWEBDL);
            caps.Categories.AddCategoryMapping(2, NewznabStandardCategory.Movies, "Short Film");
            caps.Categories.AddCategoryMapping(3, NewznabStandardCategory.TV, "Miniseries");
            caps.Categories.AddCategoryMapping(4, NewznabStandardCategory.TV, "Stand-up Comedy");
            caps.Categories.AddCategoryMapping(5, NewznabStandardCategory.TV, "Live Performance");

            return caps;
        }

        public override IParseIndexerResponse GetParser()
        {
            return new PassThePopcornParser(BaseUrl, _logger);
        }
    }
}