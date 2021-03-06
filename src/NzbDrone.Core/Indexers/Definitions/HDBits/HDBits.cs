using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBits : HttpIndexerBase<HDBitsSettings>
    {
        public override string Name => "HDBits";
        public override string BaseUrl => "https://hdbits.org";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override IndexerPrivacy Privacy => IndexerPrivacy.Private;
        public override IndexerCapabilities Capabilities => SetCapabilities();
        public override bool SupportsRedirect => true;

        public override int PageSize => 30;

        public HDBits(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger)
            : base(httpClient, indexerStatusService, configService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new HDBitsRequestGenerator() { Settings = Settings, BaseUrl = BaseUrl, Capabilities = Capabilities };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new HDBitsParser(Settings, BaseUrl);
        }

        private IndexerCapabilities SetCapabilities()
        {
            var caps = new IndexerCapabilities
            {
                TvSearchParams = new List<TvSearchParam>
                       {
                           TvSearchParam.Q, TvSearchParam.Season, TvSearchParam.Ep, TvSearchParam.TvdbId
                       },
                MovieSearchParams = new List<MovieSearchParam>
                       {
                           MovieSearchParam.Q, MovieSearchParam.ImdbId
                       }
            };

            caps.Categories.AddCategoryMapping(6, NewznabStandardCategory.Audio, "Audio Track");
            caps.Categories.AddCategoryMapping(3, NewznabStandardCategory.TVDocumentary, "Documentary");
            caps.Categories.AddCategoryMapping(8, NewznabStandardCategory.Other, "Misc/Demo");
            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.Movies, "Movie");
            caps.Categories.AddCategoryMapping(4, NewznabStandardCategory.Audio, "Music");
            caps.Categories.AddCategoryMapping(5, NewznabStandardCategory.TVSport, "Sport");
            caps.Categories.AddCategoryMapping(2, NewznabStandardCategory.TV, "TV");
            caps.Categories.AddCategoryMapping(7, NewznabStandardCategory.XXX, "XXX");

            return caps;
        }
    }
}