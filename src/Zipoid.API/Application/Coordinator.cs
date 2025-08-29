using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Zipoid.API.Application.Security;
using Zipoid.API.Domain;

namespace Zipoid.API.Application
{
    public class Coordinator : ICoordinator
    {
        private readonly ISpotifyPlaylistProvider _spotifyPlaylistProvider;
        private readonly ISearchEngine _searchEngine;
        private readonly IDownload _downloader;
        private readonly ITrackMetadataCacher _metadataCacher;

        public Coordinator(IDownload downloader, ISearchEngine searchEngine, ISpotifyPlaylistProvider spotifyPlaylistProvider, ITrackMetadataCacher metadataCacher)
        {
            _downloader = downloader;
            _searchEngine = searchEngine;
            _spotifyPlaylistProvider = spotifyPlaylistProvider;
            _metadataCacher = metadataCacher;
        }

        public async Task CoordinateAsync(string playlistUrl, Guid userId)
        {
            Console.WriteLine($"Starting coordination for user {userId} and playlist {playlistUrl}");
            var tracks = await _spotifyPlaylistProvider.GetPlaylistTracksAsync(playlistUrl);

            //download it
            var firstTenTracks = tracks.Take(10).ToList();

            //don't download it for now
            var remainderTracks = tracks.Skip(10).ToList();

            foreach (var track in firstTenTracks)
            {
                var searchResult = await _searchEngine.SearchAsync($"{track.Artist} - {track.Title}");
                if (searchResult != null)
                {
                    if (await _metadataCacher.HasFileAsync(searchResult.Query))
                    {
                        Console.WriteLine($"Track {searchResult.Query} already cached. Skipping download.");
                        continue;
                    }
                    var response = await _downloader.DownloadAudioAsync(searchResult.Url, userId);
                    var data = new StorageCache
                    {
                        Key = searchResult.Query,
                        Path = response.Path
                    };
                    await _metadataCacher.StoreMetadataAsync(data);
                }
            }

            foreach (var track in remainderTracks)
            {
                var searchResult = await _searchEngine.SearchAsync($"{track.Artist} - {track.Title}");
                if (searchResult != null)
                {
                    var data = new StorageCache
                    {
                        Key = searchResult.Query,
                        Path = "pending"
                    };
                    await _metadataCacher.StoreMetadataAsync(data);
                }
            }
        }
    }
}