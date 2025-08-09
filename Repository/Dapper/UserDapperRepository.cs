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

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns>
        /// The created user.
        /// </returns>
        public async Task<User> CreateUserAsync(User user)
        {
            var sql = "Insert into AspNetUsers (Name, Email, Phone, DealerId, PhoneNumber) Values (@Name, @Email, @Phone, @DealerId, @PhoneNumber); Select last_insert_rowid();";
            var id = await _db.ExecuteScalarAsync<int>(sql, new
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                DealerId = user.DealerId,
                PhoneNumber = user.Phone
            });
            user.Id = id;
            return user;
        }

        /// <summary>
        /// Deletes a user by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>The deleted user, or null if not found.</returns>
        /// <exception cref="Exception">Thrown when the user could not be deleted.</exception>
        public async Task<User?> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            var oldConcurrencyStamp = user.ConcurrencyStamp;
            var sql = "Delete from AspNetUsers where Id = @Id and ConcurrencyStamp = @ConcurrencyStamp";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, ConcurrencyStamp = oldConcurrencyStamp });
            if (affectedRows == 0)
            {
                throw new Exception("Something went wrong while deleting the user.");
            }
            return await GetUserByIdAsync(id);
        }


        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A list of all users.</returns>
        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var sql = "Select " +
                "u.Id as Id, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName " +
                "from AspNetUsers as u Left join Dealers as d on u.DealerId = d.Id";
            return _db.QueryAsync<User>(sql);
        }


        /// <summary>
        /// Retrieves a user by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The user with the specified ID, or null if not found.</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            var sql = "Select " +
                "u.Id as Id, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName " +
                "from AspNetUsers as u Left join Dealers as d on u.DealerId = d.Id where u.Id = @Id";
            return await _db.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        /// <summary>
        /// Updates a user's information.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="username">The new username for the user.</param>
        /// <param name="fullname">The new full name for the user.</param>
        /// <param name="email">The new email for the user.</param>
        /// <param name="phone">The new phone number for the user.</param>
        /// <returns>The updated user, or null if not found.</returns>
        public async Task<User?> UpdateUserAsync(int id, string? username, string? fullname, string? email, string? phone)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            var oldConcurrencyStamp = user.ConcurrencyStamp;
            var newConcurrencyStamp = Guid.NewGuid().ToString("D");
            var sql = "Update AspNetUsers set UserName = @UserName, Name = @Name, NormalizedUserName = @NormalizedUserName, " +
                      "Email = @Email, NormalizedEmail = @NormalizedEmail, Phone = @Phone, PhoneNumber = @PhoneNumber, " +
                      "ConcurrencyStamp = @NewConcurrencyStamp where Id = @Id and ConcurrencyStamp = @OldConcurrencyStamp";
            var affectedRows = await _db.ExecuteAsync(sql, new
            {
                Id = id,
                UserName = username,
                Name = fullname,
                NormalizedUserName = username?.ToUpper() ?? string.Empty,
                Email = email,
                NormalizedEmail = email?.ToUpper(),
                Phone = phone,
                PhoneNumber = phone,
                NewConcurrencyStamp = newConcurrencyStamp,
                OldConcurrencyStamp = oldConcurrencyStamp
            });


            if (affectedRows == 0)
            {
                throw new Exception("Something went wrong while updating the user.");
            }

            //We can also use user.Name = fullname; user.Email = email; user.Phone = phone; user.ConcurrencyStamp = newConcurrencyStamp; to avoid another query
            //But here we are returning the updated user from the database for simplicity
            return await GetUserByIdAsync(id);
        }

        /// <summary>
        /// Updates a user's dealer ID.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="dealerId">The new dealer ID for the user.</param>
        /// <returns>A tuple containing the updated user and the associated dealer, or null if not found.</returns>
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
            var updateSql = "Update AspNetUsers set DealerId = @DealerId, ConcurrencyStamp = @NewConcurrencyStamp where Id = @Id and ConcurrencyStamp = @OldConcurrencyStamp";
            var affectedRow = await _db.ExecuteAsync(updateSql, new { DealerId = dealerId, Id = id, NewConcurrencyStamp = newConcurrencyStamp, OldConcurrencyStamp = oldConcurrencyStamp });
            if (affectedRow == 0)
            {
                throw new Exception("Something went wrong while updating the user.");
            }
            return (user, dealer);
        }
    }
}