using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zipoid.API.Domain;
using Zipoid.API.Domain.Entities;
using Zipoid.API.Infrastructure.Context;

namespace Zipoid.API.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(User user)
        {
            var client = new User(user.Username, user.Email);
            client.SetPassword(user.Password);

            await _context.Users.AddAsync(client);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}