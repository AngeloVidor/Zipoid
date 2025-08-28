using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Zipoid.API.Domain;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Infrastructure.Cache
{
    public class TrackMetadataCacher : ITrackMetadataCacher
    {
        private readonly IDatabase _redis;

        public TrackMetadataCacher(IDatabase redis)
        {
            _redis = redis;
        }

        public async Task StoreMetadataAsync(IEnumerable<Track> tracks)
        {
            var tasks = new List<Task>();

            foreach (var track in tracks)
            {
                var key = $"track:{track.Id}";
                var entries = new HashEntry[]
                {
                new HashEntry("artist", track.Artist),
                new HashEntry("title", track.Title),
                new HashEntry("album", track.Album)
                };

                tasks.Add(_redis.HashSetAsync(key, entries));
            }

            await Task.WhenAll(tasks);
        }

        public async Task<List<Track>> GetMetadataAsync(IEnumerable<string> trackIds)
        {
            var tasks = trackIds.Select(async id =>
            {
                var key = $"track:{id}";
                var entries = await _redis.HashGetAllAsync(key);

                if (entries.Length == 0)
                    return null;

                return new Track
                {
                    Id = id,
                    Artist = entries.FirstOrDefault(e => e.Name == "artist").Value,
                    Title = entries.FirstOrDefault(e => e.Name == "title").Value,
                    Album = entries.FirstOrDefault(e => e.Name == "album").Value
                };
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(t => t != null).ToList()!;
        }

    }
}