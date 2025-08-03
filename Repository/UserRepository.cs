using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> DeleteUserAsync(int id)
        {
            var userToDelete = await _context.Users.FindAsync(id);
            if (userToDelete == null)
            {
                return userToDelete;
            }

            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();

            return userToDelete;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.Include(u => u.Dealer).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.Include(u => u.Dealer).FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task<User?> UpdateUserAsync(int id, string? name, string? email, string? phone)
        {
            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                return userToUpdate;
            }

            userToUpdate.Name = name ?? userToUpdate.Name;
            userToUpdate.Email = email ?? userToUpdate.Email;
            userToUpdate.Phone = phone ?? userToUpdate.Phone;

            await _context.SaveChangesAsync();

            return userToUpdate;

        }

        public async Task<User?> UpdateUserDealerIdAsync(int id, int DealerId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return user;
            }

            user.DealerId = DealerId;

            await _context.SaveChangesAsync();

            return user;

        }
    }
}