using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zipoid.API.Application.Security;
using Zipoid.API.Domain;

namespace Zipoid.API.Application
{
    public class Coordinator : ICoordinator
    {
        private readonly ISpotifyPlaylistProvider _spotifyPlaylistProvider;
        private readonly ISearchEngine _searchEngine;
        private readonly IDownload _downloader;

        public Coordinator(IDownload downloader, ISearchEngine searchEngine, ISpotifyPlaylistProvider spotifyPlaylistProvider)
        {
            _downloader = downloader;
            _searchEngine = searchEngine;
            _spotifyPlaylistProvider = spotifyPlaylistProvider;
        }

        public async Task CoordinateAsync(string playlistUrl, Guid userId)
        {
            Console.WriteLine($"Starting coordination for user {userId} and playlist {playlistUrl}");
            var tracks = await _spotifyPlaylistProvider.GetPlaylistTracksAsync(playlistUrl);
            foreach (var track in tracks)
            {
                var searchResult = await _searchEngine.SearchAsync($"{track.Artist} - {track.Title}");
                if (searchResult != null)
                {
                    await _downloader.DownloadAudioAsync(searchResult, userId);
                }
            }
        }
    }
}