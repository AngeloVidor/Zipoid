using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Services;
using Models;

namespace Zipoid.API.Domain
{
    public interface ISearchEngine
    {
        Task<Search> SearchAsync(string query);
    }
}