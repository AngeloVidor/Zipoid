using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Zipoid.API.Domain;

namespace Zipoid.API.Infrastructure.Spotify
{
    public class SpotifyTokenProvider : ISpotifyTokenProvider
    {
        private readonly HttpClient _httpClient;
        private readonly Models.Spotify _spotifySettings;
        private const string BaseUrl = "https://accounts.spotify.com/api/token";

        public SpotifyTokenProvider(HttpClient httpClient, Models.Spotify spotifySettings)
        {
            _httpClient = httpClient;
            _spotifySettings = spotifySettings;
        }

        public async Task<string> GetTokenAsync()
        {
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_spotifySettings.Client_ID}:{_spotifySettings.Client_Secret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            };

            var content = new FormUrlEncodedContent(body);

            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();

            return token!;
        }

    }

}
