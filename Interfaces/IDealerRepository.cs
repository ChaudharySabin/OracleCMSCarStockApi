using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IDealerRepository
    {
        Task<IEnumerable<Dealer>> GetAllDealersAsync();

        Task<Dealer?> GetDealerByIdAsync(int id);

        Task<Dealer> CreateDealerAsync(Dealer dealer);

        Task<Dealer?> UpdateDealerAsync(int id, string name, string? description);

        Task<Dealer?> DeleteDealerAsync(int id);

    }
}