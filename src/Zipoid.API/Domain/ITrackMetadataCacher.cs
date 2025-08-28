using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Domain
{
    public interface ITrackMetadataCacher
    {
        Task StoreMetadataAsync(IEnumerable<Track> tracks);
        Task<List<Track>> GetMetadataAsync(IEnumerable<string> trackIds);
    }
}