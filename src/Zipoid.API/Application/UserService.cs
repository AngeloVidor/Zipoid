using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zipoid.API.Domain;
using Zipoid.API.Domain.Entities;

namespace Zipoid.API.Application
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> AddAsync(User user)
        {
            var result = await _userRepository.AddAsync(user);
            if (!result) throw new Exception("Failed to add user");
            return result;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var client = await _userRepository.GetUserByEmailAsync(email);

            if (client == null || !client.CheckPassword(password))
            {
                throw new Exception("Invalid email or password");
            }
            return true;
        }
    }
}