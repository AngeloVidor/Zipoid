using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Domain
{
    public interface ISpotifyPlaylistProvider
    {
        Task<IEnumerable<Track>> GetPlaylistTracksAsync(string playlistId);
    }
}