using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Zipoid.API.Domain;

namespace Zipoid.API.Application
{
    public class OnDemandService : IOnDemand
    {
        private readonly ITrackMetadataCacher _trackMetadataCacher;
        private readonly IDownload _downloader;
        private readonly ISearchEngine _searchEngine;

        public OnDemandService(ITrackMetadataCacher trackMetadataCacher, IDownload downloader, ISearchEngine searchEngine)
        {
            _trackMetadataCacher = trackMetadataCacher;
            _downloader = downloader;
            _searchEngine = searchEngine;
        }

        public async Task HandleAsync(string key, Guid userId)
        {
            var track = await _trackMetadataCacher.GetTrackByKeyAsync(key);
            if (track is not null && track.Path == "pending")
            {
                var searchResult = await _searchEngine.SearchAsync(key);
                if (searchResult != null)
                {
                    if (await _trackMetadataCacher.HasFileAsync(searchResult.Query))
                    {
                        Console.WriteLine($"Track {searchResult.Query} already cached. Skipping download.");
                        return;
                    }

                    Console.WriteLine($"OnDemand: Downloading track {searchResult.Query} for user {userId}");
                    var response = await _downloader.DownloadAudioAsync(searchResult.Url, userId);
                    var data = new StorageCache
                    {
                        Key = searchResult.Query,
                        Path = response.Path
                    };
                    await _trackMetadataCacher.StoreMetadataAsync(data);
                }
            }
        }
    }
}