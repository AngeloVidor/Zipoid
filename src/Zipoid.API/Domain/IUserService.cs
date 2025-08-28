using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Domain
{
    public interface IUserService
    {
        Task<bool> AddAsync(User user);
        Task<bool> LoginAsync(string email, string password);
    }
}