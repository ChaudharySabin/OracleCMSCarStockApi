using System.Data;
using api.Interfaces;
using api.Models;
using Dapper;
namespace api.Repository.Dapper
{
    public class UserDapperRepository : IUserRepository
    {
        private readonly IDbConnection _db;

        public UserDapperRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var sql = "Insert into Users (Name, Email, Phone, DealerId) Values (@Name, @Email, @Phone, @DealerId); Select last_insert_rowid();";
            var id = await _db.ExecuteScalarAsync<int>(sql, new { user.Name, user.Email, user.Phone, user.DealerId });
            user.Id = id;
            return user;
        }

        public async Task<User?> DeleteUserAsync(int id)
        {
            var sql = "Delete from Users where Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id });
            if (affectedRows == 0)
            {
                return null;
            }
            return await GetUserByIdAsync(id);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var sql = "Select " +
                "u.Id as Id, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName " +
                "from Users as u Left join Dealers as d on u.DealerId = d.Id";
            return _db.QueryAsync<User>(sql);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var sql = "Select " +
                "u.Id as Id, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName " +
                "from Users as u Left join Dealers as d on u.DealerId = d.Id where u.Id = @Id";
            return await _db.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> UpdateUserAsync(int id, string? username, string? fullname, string? email, string? phone)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            var oldConcurrencyStamp = user.ConcurrencyStamp;
            var newConcurrencyStamp = Guid.NewGuid().ToString("D");
            var sql = "Update Users set UserName = @UserName, Name = @Name, NormalizedUserName = @NormalizedUserName, " +
                      "Email = @Email, NormalizedEmail = @NormalizedEmail, SecurityStamp = @SecurityStamp, " +
                      "ConcurrencyStamp = @ConcurrencyStamp where Id = @Id and ConcurrencyStamp = @ConcurrencyStamp";
            var affectedRows = await _db.ExecuteAsync(sql, new
            {
                Id = id,
                UserName = username,
                Name = fullname,
                Email = email,
                Phone = phone,
                NormalizedUserName = username!.ToUpper(),
                NormalizedEmail = email?.ToUpper(),
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = newConcurrencyStamp
            });


            if (affectedRows == 0)
            {
                return null;
            }
            return await GetUserByIdAsync(id);
        }

        public async Task<(User?, Dealer?)> UpdateUserDealerIdAsync(int id, int dealerId)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return (null, null);
            }
            var dealer = await _db.QuerySingleOrDefaultAsync<Dealer>("Select * from Dealers where Id = @DealerId", new { DealerId = dealerId });
            if (dealer == null)
            {
                return (user, null);
            }
            user.DealerId = dealerId;
            var oldConcurrencyStamp = user.ConcurrencyStamp;
            var newConcurrencyStamp = Guid.NewGuid().ToString("D");
            var updateSql = "Update Users set DealerId = @DealerId, ConcurrencyStamp = @ConcurrencyStamp where Id = @Id and ConcurrencyStamp = @OldConcurrencyStamp";
            var affectedRow = await _db.ExecuteAsync(updateSql, new { DealerId = dealerId, Id = id, ConcurrencyStamp = newConcurrencyStamp, OldConcurrencyStamp = oldConcurrencyStamp });
            if (affectedRow == 0)
            {
                return (null, null);
            }
            return (user, dealer);
        }
    }
}