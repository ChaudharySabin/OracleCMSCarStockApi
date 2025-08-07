using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using SQLitePCL;
using Dapper;

namespace api.Repository.Dapper
{
    public class DealerDapperRepository : IDealerRepository
    {
        private readonly IDbConnection _db;

        public DealerDapperRepository(IDbConnection db)
        {
            _db = db;
        }
        public async Task<Dealer> CreateDealerAsync(Dealer dealer)
        {
            var sql = "Insert into Dealers (Name, Description) Values (@Name, @Description); Select last_insert_rowid();";
            var id = await _db.ExecuteScalarAsync<int>(sql, new { dealer.Name, dealer.Description });
            dealer.Id = id;
            return dealer;
        }

        public async Task<Dealer?> DeleteDealerAsync(int id)
        {
            var sql = "Delete from Dealers where Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id });
            if (affectedRows == 0)
            {
                return null;
            }
            return new Dealer { Id = id };
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            var sql = "Select Id, Name, Description from Dealers";
            return await _db.QueryAsync<Dealer>(sql);
        }

        public async Task<Dealer?> GetDealerByIdAsync(int id)
        {
            var sql = "Select Id, Name, Description from Dealers where Id = @Id";
            return await _db.QuerySingleOrDefaultAsync<Dealer>(sql, new { Id = id });
        }


        public async Task<Dealer?> UpdateDealerAsync(int id, string name, string? description)
        {
            string sql = "Update Dealers set Name=@Name, Description = @description where Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, Name = name, Description = description });
            if (affectedRows == 0)
            {
                return null;
            }
            return await GetDealerByIdAsync(id);
        }
    }
}