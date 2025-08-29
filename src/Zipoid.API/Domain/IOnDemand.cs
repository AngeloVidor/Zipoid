using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zipoid.API.Domain
{
    public interface IOnDemand
    {
        Task HandleAsync(string key, Guid userId);
    }
}