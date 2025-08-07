using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.EFcore.Repository
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
            // return await _context.Users.Include(u => u.Dealer).ToListAsync();
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task<User?> UpdateUserAsync(int id, string? username, string? fullname, string? email, string? phone)
        {
            var userToUpdate = await _context.Users.FindAsync(id);
            if (userToUpdate == null)
            {
                return userToUpdate;
            }

            userToUpdate.Name = fullname ?? userToUpdate.Name;
            userToUpdate.Email = email ?? userToUpdate.Email;
            userToUpdate.NormalizedEmail = email?.ToUpper() ?? userToUpdate.NormalizedEmail;
            userToUpdate.Phone = phone ?? userToUpdate.Phone;
            userToUpdate.PhoneNumber = phone ?? userToUpdate.PhoneNumber;
            await _context.SaveChangesAsync();

            return userToUpdate;

        }

        public async Task<(User?, Dealer?)> UpdateUserDealerIdAsync(int id, int dealerId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return (user, null);
            }

            var dealer = await _context.Dealers.FindAsync(dealerId);
            if (dealer == null)
            {
                return (user, dealer);
            }

            user.DealerId = dealerId;

            await _context.SaveChangesAsync();

            return (user, dealer);

        }
    }
}