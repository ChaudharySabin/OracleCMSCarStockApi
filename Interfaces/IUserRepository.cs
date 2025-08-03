using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(int id);

        Task<User> CreateUserAsync(User user);

        Task<User?> DeleteUserAsync(int id);

        Task<User?> UpdateUserAsync(int id, string? name, string? email, string? phone);

        Task<User?> UpdateUserDealerIdAsync(int id, int DealerId);
    }
}