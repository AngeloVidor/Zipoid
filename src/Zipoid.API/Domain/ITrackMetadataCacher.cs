using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Extensions.Msal;
using Models;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Domain
{
    public interface ITrackMetadataCacher
    {
        Task StoreMetadataAsync(StorageCache data);
        Task<List<StorageCache>> GetMetadataAsync();
        Task<bool> HasFileAsync(string key);
        Task<StorageCache?> GetTrackByKeyAsync(string key);
    }
}