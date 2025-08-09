using System.Data;
using api.Interfaces;
using api.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;
namespace api.Repository.Dapper
{
    public class UserDapperRepository : IUserRepository
    {
        private readonly IDbConnection _db;
        private readonly UserManager<User> _userManager;

        public UserDapperRepository(IDbConnection db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
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
            // This method will almost newer be used as we are using Identity to create users. This is here is just an example of how to use Dapper to create a user.
            // The acutal dapper implementation is the created in the UserDapperStore class.
            var passwordHasher = new PasswordHasher<User>();
            var hashedPasswpord = passwordHasher.HashPassword(user, user.PasswordHash!);
            // We are using Identity to create users, so we don't need to insert the user into the database manually.
            // But we are still using Dapper to insert the user into the database.
            user.PasswordHash = hashedPasswpord;
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            var sql = "Insert into AspNetUsers (UserName, NormalizedUserName, Name, Email, NormalizedEmail,Phone,PhoneNumber, PasswordHash, ConcurrencyStamp, DealerId) Values (@UserName, @NormalizedUserName, @Name, @Email, @NormalizedEmail, @Phone, @PhoneNumber, @PasswordHash, @ConcurrencyStamp, @DealerId); Select last_insert_rowid();";
            var id = await _db.ExecuteScalarAsync<int>(sql, new
            {
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                Name = user.Name,
                Email = user.Email,
                NormalizedEmail = user.NormalizedEmail,
                Phone = user.Phone,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = user.PasswordHash,
                ConcurrencyStamp = user.ConcurrencyStamp,
                DealerId = user.DealerId
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
            //We need not to check for ConcurrencyStamp as users are being created by Identity and it will always have a ConcurrencyStamp.
            // However, we are checking it here for edge cases where the IUserRepository is used directly.
            var sql = "Delete from AspNetUsers where Id = @Id and (ConcurrencyStamp = @ConcurrencyStamp or ConcurrencyStamp is null);";
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
                "u.Id as Id, u.UserName as UserName, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName, r.Name as RoleName " +
                "from AspNetUsers as u Left join Dealers as d on u.DealerId = d.Id " +
                "left join AspNetUserRoles as ur on u.Id = ur.UserId " +
                "left join AspNetRoles as r on ur.RoleId = r.Id;";
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
                "u.Id as Id, u.UserName as UserName, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName, r.Name as RoleName, u.ConcurrencyStamp as ConcurrencyStamp " +
                "from AspNetUsers as u Left join Dealers as d on u.DealerId = d.Id left join AspNetUserRoles as ur on u.Id = ur.UserId " +
                "left join AspNetRoles as r on ur.RoleId = r.Id " +
                "where u.Id = @Id;";
            return await _db.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {

            var sql = "Select " +
                "u.Id as Id, u.UserName as UserName, u.Name as Name, u.Email as Email, u.Phone as Phone, u.DealerId, d.Name as DealerName, r.Name as RoleName " +
                "from AspNetUsers as u Left join Dealers as d on u.DealerId = d.Id " +
                "left join AspNetUserRoles as ur on u.Id = ur.UserId " +
                "left join AspNetRoles as r on ur.RoleId = r.Id " +
                "where r.NormalizedName like @RoleName;";

            return await _db.QueryAsync<User>(sql, new { RoleName = $"%{roleName.ToUpperInvariant()}%" });
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
                      "ConcurrencyStamp = @NewConcurrencyStamp where Id = @Id and (ConcurrencyStamp = @OldConcurrencyStamp or ConcurrencyStamp is null);";
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
            var dealer = await _db.QuerySingleOrDefaultAsync<Dealer>("Select * from Dealers where Id = @DealerId;", new { DealerId = dealerId });
            if (dealer == null)
            {
                return (user, null);
            }
            user.DealerId = dealerId;
            user.DealerName = dealer.Name ?? string.Empty;
            var oldConcurrencyStamp = user.ConcurrencyStamp;
            var newConcurrencyStamp = Guid.NewGuid().ToString();
            var updateSql = "Update AspNetUsers set DealerId = @DealerId, ConcurrencyStamp = @NewConcurrencyStamp where Id = @Id and (ConcurrencyStamp = @OldConcurrencyStamp or ConcurrencyStamp is null);";
            var affectedRow = await _db.ExecuteAsync(updateSql, new { DealerId = dealerId, Id = id, NewConcurrencyStamp = newConcurrencyStamp, OldConcurrencyStamp = oldConcurrencyStamp });
            if (affectedRow == 0)
            {
                throw new Exception("Something went wrong while updating the user.");
            }
            return (user, dealer);
        }
    }
}