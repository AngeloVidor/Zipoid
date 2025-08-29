using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Extensions.Msal;
using Models;
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

        public async Task StoreMetadataAsync(StorageCache data)
        {
            var tasks = new List<Task>();

            var key = $"track:{data.Key}";
            var entries = new HashEntry[]
            {
                new HashEntry("Path", data.Path),
            };

            tasks.Add(_redis.HashSetAsync(key, entries));

            await Task.WhenAll(tasks);
        }

        public async Task<List<StorageCache>> GetMetadataAsync()
        {
            var endpoints = _redis.Multiplexer.GetEndPoints();
            var server = _redis.Multiplexer.GetServer(endpoints.First());

            var keys = server.Keys(pattern: "track:*").ToArray();

            var tasks = keys.Select(async key =>
            {
                var entries = await _redis.HashGetAllAsync(key);
                if (entries.Length == 0)
                    return null;

                return new StorageCache
                {
                    Key = key.ToString().Replace("track:", ""),
                    Path = entries.FirstOrDefault(e => e.Name == "Path").Value
                };
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(t => t != null).ToList()!;
        }



    }
}