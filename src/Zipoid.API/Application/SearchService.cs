using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using Zipoid.API.Domain;

namespace Zipoid.API.Application
{
    public class SearchService : ISearchEngine
    {
        public async Task<string> SearchAsync(string query)
        {
            var youtube = new YoutubeClient();
            var result = await youtube.Search.GetVideosAsync(query).FirstAsync();
            return result.Url;
        }
    }
}