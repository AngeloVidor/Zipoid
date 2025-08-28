using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Zipoid.API.Domain;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Infrastructure.Spotify.Playlist
{
    public class SpotifyPlaylistProvider : ISpotifyPlaylistProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ISpotifyTokenProvider _tokenProvider;

        public SpotifyPlaylistProvider(HttpClient httpClient, ISpotifyTokenProvider tokenProvider)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;
        }

        public async Task<IEnumerable<Track>> GetPlaylistTracksAsync(string playlistId)
        {
            var token = await _tokenProvider.GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"playlists/{playlistId}/tracks");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var items = doc.RootElement.GetProperty("items");
            var result = new List<Track>();

            int counter = 0;

            foreach (var item in items.EnumerateArray())
            {
                counter++;
                var track = item.GetProperty("track");
                var trackName = track.GetProperty("name").GetString() ?? "";
                var albumName = track.GetProperty("album").GetProperty("name").GetString() ?? "";

                var artist = track.GetProperty("artists")[0].GetProperty("name").GetString() ?? "";

                result.Add(new Track
                {
                    Id = counter.ToString(),
                    Title = trackName,
                    Album = albumName,
                    Artist = artist
                });
            }



            return result;
        }
    }
}