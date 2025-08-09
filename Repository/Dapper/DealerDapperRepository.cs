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

        /// <summary>
        /// Creates a new dealer in the database.
        /// </summary>
        /// <param name="dealer">The dealer to create.</param>
        /// <returns>
        /// The created dealer.
        /// </returns>
        public async Task<Dealer> CreateDealerAsync(Dealer dealer)
        {
            var concurrencyStamp = Guid.NewGuid().ToString();
            var sql = "Insert into Dealers (Name, Description, ConcurrencyStamp) Values (@Name, @Description, @ConcurrencyStamp); Select last_insert_rowid();";
            var id = await _db.ExecuteScalarAsync<int>(sql, new { Name = dealer.Name, Description = dealer.Description, ConcurrencyStamp = concurrencyStamp });
            dealer.Id = id;
            return dealer;
        }

        /// <summary>
        /// Deletes a dealer by its ID.
        /// </summary>
        /// <param name="id">The ID of the dealer to delete.</param>
        /// <returns>The deleted dealer, or null if not found.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Dealer?> DeleteDealerAsync(int id)
        {
            Dealer? dealer = await GetDealerByIdAsync(id);
            if (dealer == null)
            {
                return null;
            }

            // Cascading delete is not on by default in SQLite, so we need to handle it manually.
            // We are dealing with multiple tables, so we are creating a transaction and commiting it only if everything goes well.
            // If something goes wrong, we will rollback the transaction.
            using var transaction = _db.BeginTransaction();
            try
            {
                // Delete all cars associated with the dealer
                var deleteCarsSql = "Delete from Cars where DealerId = @DealerId;";
                var result = await _db.ExecuteAsync(deleteCarsSql, new { DealerId = id }, transaction);

                // Checking if there are any users associated with the dealer
                var usersSql = "Select count(*) from AspNetUsers where DealerId = @DealerId;";
                var userCount = await _db.ExecuteScalarAsync<int>(usersSql, new { DealerId = id });
                if (userCount > 0)
                {
                    const string clearUsers = "UPDATE AspNetUsers SET DealerId = NULL WHERE DealerId = @Id;";
                    await _db.ExecuteAsync(clearUsers, new { Id = id }, transaction);
                }

                var oldConcurrencyStamp = dealer.ConcurrencyStamp;
                // We are also checking null as ConcurrencyStamp can be null in the database and a simple ConcurrencyStamp = null will never be true 
                var sql = "Delete from Dealers where Id = @Id and (ConcurrencyStamp = @ConcurrencyStamp or ConcurrencyStamp is null);";
                var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, ConcurrencyStamp = oldConcurrencyStamp }, transaction);
                if (affectedRows == 0)
                {
                    throw new DBConcurrencyException("Dealer was modified or does not exist.");
                }


                transaction.Commit();
                return new Dealer { Id = id };
            }
            catch (Exception)
            {
                transaction.Rollback();
                // Console.WriteLine(ex.Message);
                throw new Exception("Something went wrong when deleting the dealer."); // This error will be sent to the controller to show server error
            }
        }

        /// <summary>
        /// Retrieves all dealers from the database.
        /// </summary>
        /// <returns>A list of all dealers.</returns>
        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            var sql = "Select Id, Name, Description from Dealers;";
            return await _db.QueryAsync<Dealer>(sql);
        }

        /// <summary>
        /// Retrieves a dealer by its ID.
        /// </summary>
        /// <param name="id">The ID of the dealer to retrieve.</param>
        /// <returns>The dealer with the specified ID, or null if not found.</returns>
        public async Task<Dealer?> GetDealerByIdAsync(int id)
        {
            var sql = "Select Id, Name, Description, ConcurrencyStamp from Dealers where Id = @Id;";
            return await _db.QuerySingleOrDefaultAsync<Dealer>(sql, new { Id = id });
        }

        /// <summary>
        /// Updates a dealer's information.
        /// </summary>
        /// <param name="id">The ID of the dealer to update.</param>
        /// <param name="name">The new name of the dealer.</param>
        /// <param name="description">The new description of the dealer.</param>
        /// <returns>The updated dealer, or null if not found.</returns>
        public async Task<Dealer?> UpdateDealerAsync(int id, string name, string? description)
        {
            var dealer = await GetDealerByIdAsync(id);
            if (dealer == null)
            {
                return null;
            }

            var oldConcurrencyStamp = dealer.ConcurrencyStamp;
            var newConcurrencyStamp = Guid.NewGuid().ToString();
            // We are also checking null as ConcurrencyStamp can be null in the database and a simple ConcurrencyStamp = null will never be true 
            string sql = "Update Dealers set Name=@NewName, Description = @NewDescription, ConcurrencyStamp = @NewConCurrencyStamp where Id = @Id and (ConcurrencyStamp = @OldConcurrencyStamp or ConcurrencyStamp is null);";
            var affectedRows = await _db.ExecuteAsync(sql, new
            {
                Id = id,
                NewName = name,
                NewDescription = description,
                NewConCurrencyStamp = newConcurrencyStamp,
                OldConcurrencyStamp = oldConcurrencyStamp
            });


            if (affectedRows == 0)
            {
                return null;
            }

            return await GetDealerByIdAsync(id);  // Or we can use dealer.Name = name; dealer.Description = description;, dealer.ConcurrencyStamp = newConcurrencyStamp; in order to avoid another query
        }
    }
}