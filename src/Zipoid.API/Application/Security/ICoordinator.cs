using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zipoid.API.Application.Security
{
    public interface ICoordinator
    {
        Task CoordinateAsync(string playlistUrl, Guid userId);
    }
}