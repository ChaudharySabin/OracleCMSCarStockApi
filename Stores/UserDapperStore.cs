using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Dapper;
using api.Models;
using System.Diagnostics;
using System.Data.Common;

namespace api.Stores
{
    public class UserDapperStore :
    IUserStore<User>, // This interface is used to store user information in the database.
    IUserPasswordStore<User>, // This interface is used to store user passwords in the database.
    IUserEmailStore<User>, // This interface is used to store user emails in the database.
    IUserSecurityStampStore<User>, // This interface is used to store user security stamps in the database.
    IUserRoleStore<User>, // This interface is used to store user roles in the database.
    IUserPhoneNumberStore<User> // This interface is used to store user phone numbers in the database. Mainly used here to generate otp codes for password reset instead of tokens.
    {
        private readonly IDbConnection _db;
        public UserDapperStore(IDbConnection db)
        {
            _db = db;
        }
        // The Identity follows a pattern called Unit of Work pattern, which essentially means that we change the state of the user in memory and then save it to the database.
        // Getting and setting of a single user properties is done without interacting with the database
        // So CreateAsync, UpdateAsync, DeleteAsync, etc. are methods that save it to the database.

        //Methods that interact with the database
        public async Task<IdentityResult> AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            var sql = "Select Id from AspNetRoles where NormalizedName = @NormalizedName;";
            var roleId = await _db.ExecuteScalarAsync<int?>(sql, new { NormalizedName = roleName.ToUpperInvariant() }); //Rolenames are nor
            if (roleId == null) throw new ArgumentException("Role not found.", nameof(roleName));

            var insertSql = "Insert into AspNetUserRoles (UserId, RoleId) Values (@UserId, @RoleId);";
            var result = await _db.ExecuteAsync(insertSql, new { UserId = user.Id, RoleId = roleId });
            return result == 0 ? IdentityResult.Failed(new IdentityError
            {
                Code = nameof(AddToRoleAsync),
                Description = "Failed to add user to role."
            }) : IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default)
        {

            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            // user.SecurityStamp = Guid.NewGuid().ToString("D");
            user.ConcurrencyStamp = Guid.NewGuid().ToString("D");

            var sql = "Insert into AspNetUsers (UserName, Name, NormalizedUserName, Email, NormalizedEmail, PasswordHash, SecurityStamp, Phone , ConcurrencyStamp, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount) " +
                       "Values (@UserName, @Name, @NormalizedUserName, @Email, @NormalizedEmail, @PasswordHash, @SecurityStamp, @Phone, @ConcurrencyStamp, @EmailConfirmed, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnabled, @AccessFailedCount);" +
                       " SELECT last_insert_rowid();";
            var rowid = await _db.ExecuteScalarAsync<int>(sql, new
            {
                user.UserName,
                user.Name,
                user.NormalizedUserName,
                user.Email,
                user.NormalizedEmail,
                user.PasswordHash,
                user.SecurityStamp,
                user.Phone,
                user.ConcurrencyStamp,
                EmailConfirmed = user.EmailConfirmed ? 1 : 0, // SQLite uses 1 for true and 0 for false
                PhoneNumberConfirmed = user.PhoneNumberConfirmed ? 1 : 0,
                TwoFactorEnabled = user.TwoFactorEnabled ? 1 : 0, // SQLite uses 1 for true and 0 for false
                LockoutEnabled = user.LockoutEnabled ? 1 : 0, // SQLite uses 1 for true and 0 for false
                AccessFailedCount = user.AccessFailedCount
            });
            user.Id = rowid;
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            // This will be handled by the UserDapperRepository in the context of the application
            // but here we can implement the logic to delete a user from the database.
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            var sql = "Delete from AspNetUsers where Id = @Id;";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = user.Id });
            return affectedRows == 1 ? IdentityResult.Success :
                IdentityResult.Failed(
                    new IdentityError
                    { Code = nameof(DeleteAsync), Description = "User deletion failed." }
                );
        }

        public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(normalizedEmail)) throw new ArgumentNullException(nameof(normalizedEmail));
            // Console.WriteLine($"Finding user by email: {normalizedEmail}");
            var sql = "Select * from AspNetUsers where NormalizedEmail = @NormalizedEmail;";
            return _db.QuerySingleOrDefaultAsync<User>(sql, new { NormalizedEmail = normalizedEmail });
        }

        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (int.TryParse(userId, out int id) == false)
            {
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            }
            var sql = "Select * from AspNetUsers where Id = @Id;";
            return _db.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(normalizedUserName)) throw new ArgumentNullException(nameof(normalizedUserName));
            var sql = "Select * from AspNetUsers where NormalizedUserName = @NormalizedUserName;";
            return _db.QuerySingleOrDefaultAsync<User>(sql, new { NormalizedUserName = normalizedUserName });
        }

        public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));

            // Can also be done using the query below
            // var sql = "Delete from AspNetUserRoles where UserId = @UserId
            // and RoleId = (Select Id from AspNetRoles where NormalizedName = @roleName)";
            // But we are using the roleName to get the roleId first and then deleting the user from the role.
            //The roleName is normalized to upper case from the user Manager. 

            // This query is used to make it more readable
            var roleId = await _db.ExecuteScalarAsync<int?>(
                "Select Id from AspNetRoles where NormalizedName = @NormalizedName;",
                new { NormalizedName = roleName.ToUpperInvariant() });
            if (roleId == null)
            {
                throw new ArgumentException("Role not found.", nameof(roleName));
            }
            var sql = "Delete from AspNetUserRoles where UserId = @UserId and RoleId = @RoleId;";
            var result = await _db.ExecuteAsync(sql, new
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            var oldConcurrencyStamp = user.ConcurrencyStamp;
            var newConcurrencyStamp = Guid.NewGuid().ToString("D");

            var sql = "Update AspNetUsers set UserName = @UserName, Name = @Name, NormalizedUserName = @NormalizedUserName, " +
                      "Email = @Email, PasswordHash = @PasswordHash, NormalizedEmail = @NormalizedEmail, SecurityStamp = @SecurityStamp, " +
                      "ConcurrencyStamp = @ConcurrencyStamp where Id = @Id and (ConcurrencyStamp = @OldConcurrencyStamp or ConcurrencyStamp is null);";

            var affectedRows = await _db.ExecuteAsync(sql, new
            {
                UserName = user.UserName,
                Name = user.Name,
                NormalizedUserName = user.NormalizedUserName,
                NormalizedEmail = user.NormalizedEmail,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = newConcurrencyStamp,
                OldConcurrencyStamp = oldConcurrencyStamp,
                Id = user.Id
            });

            user.ConcurrencyStamp = newConcurrencyStamp; // Update the concurrency stamp in memory
            return affectedRows == 1 ? IdentityResult.Success :
                IdentityResult.Failed(new IdentityError
                {
                    Code = "ConcurrencyFailure",
                    Description = "The user was modified by another process."
                });
        }

        public void Dispose()
        {
            // Dispose of the database connection if necessary
            if (_db is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }






        // Methods that don't interact with the database

        public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {

            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.EmailConfirmed)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string?> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException();
            return Task.FromResult(user?.Phone);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException();
            return Task.FromResult(user.PhoneNumberConfirmed);

        }

        // This method is an exception here as Dapper doesn't provide functionality to include relationships like Entity Framework does.
        // Hence, we fetch the roles from the database directly.
        public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            var sql = "Select r.Name from AspNetUserRoles as ur join AspNetRoles r on ur.RoleId = r.Id where ur.UserId = @UserId";
            var results = await _db.QueryAsync<string>(sql, new { UserId = user.Id });
            return results.ToList();
        }

        public Task<string?> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.UserName);
        }


        // Similarly we are using database here to check all users in a role.
        // This is because Dapper doesn't provide functionality to include relationships like Entity Framework does.
        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));
            var sql = "Select u.* from AspNetUsers as u join AspNetUserRoles as ur on u.Id = ur.userId join AspNetRoles as r on ur.RoleId = r.Id where r.NormalizedName = roleName";
            IEnumerable<User> users = await _db.QueryAsync<User>(sql, new { roleName = roleName.ToUpperInvariant() });
            return users.ToList();
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }


        // Similarly we are using database here to check if the user is in a role.
        // This is because Dapper doesn't provide functionality to include relationships like Entity Framework does.
        public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentNullException(nameof(roleName));
            var sql = "select count(*) from AspNetUserRoles as ur " +
                      "join AspNetRoles as r on ur.RoleId = r.Id " +
                      "where ur.UserId = @UserId and r.NormalizedName = @NormalizedName";
            var count = await _db.ExecuteScalarAsync<int>(sql, new
            {
                UserId = user.Id,
                NormalizedName = roleName.ToUpperInvariant()
            });
            return count == 0 ? false : true;

        }

        public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
        {
            //This mainly based on the implementaion of simonfaltum/ASPNETCoreIdentityDapperStore
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.Email = email;
            return Task.CompletedTask;

        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(User user, string? phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException();
            user.PhoneNumber = phoneNumber;
            user.Phone = phoneNumber;
            return Task.CompletedTask;

        }

        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException();
            user.PhoneNumberConfirmed = true;
            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null) throw new ArgumentNullException(nameof(user));
            user.UserName = userName;
            return Task.CompletedTask;
        }


        Task IUserRoleStore<User>.AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            return AddToRoleAsync(user, roleName, cancellationToken);
        }
    }
}