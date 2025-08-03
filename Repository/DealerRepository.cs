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
    public class DealerRepository : IDealerRepository
    {
        private readonly ApplicationDbContext _context;

        public DealerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dealer> CreateDealerAsync(Dealer dealer)
        {
            await _context.Dealers.AddAsync(dealer);
            await _context.SaveChangesAsync();
            return dealer;
        }

        public async Task<Dealer?> DeleteDealerAsync(int id)
        {
            Dealer? dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null)
            {
                return null;
            }

            _context.Dealers.Remove(dealer);

            await _context.SaveChangesAsync();

            return dealer;
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _context.Dealers.ToListAsync();
        }

        public async Task<Dealer?> GetDealerByIdAsync(int id)
        {
            return await _context.Dealers.FindAsync(id);


        }

        public async Task<Dealer?> UpdateDealerAsync(int id, string name, string? description)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null)
            {
                return dealer;
            }

            dealer.Name = name;
            dealer.Description = description ?? dealer.Description;

            await _context.SaveChangesAsync();

            return dealer;

        }
    }
}