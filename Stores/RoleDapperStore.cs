using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Dapper;

namespace api.Stores
{
    public class RoleDapperStore : IRoleStore<IdentityRole<int>>
    {
        private readonly IDbConnection _db;
        public RoleDapperStore(IDbConnection db)
        {
            _db = db;
        }
        public async Task<IdentityResult> CreateAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            var concurrenyStamp = new Guid().ToString();
            var sql = "INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp) VALUES (@Name, @NormalizedName, @ConcurrencyStamp);SElect last_insert_rowid();";
            int result = await _db.ExecuteAsync(sql, new
            {
                Name = role.Name,
                NormalizedName = role.NormalizedName,
                ConcurrencyStamp = concurrenyStamp
            });

            role.Id = result;
            role.ConcurrencyStamp = concurrenyStamp;
            return result == 0 ? IdentityResult.Failed(new IdentityError
            {
                Description = "Role creation failed.",
                Code = "RoleCreationFailed"
            }) : IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var sql = "DELETE FROM AspNetRoles WHERE Id = @Id AND (ConcurrencyStamp = @ConcurrencyStamp OR ConcurrencyStamp IS NULL);";
            var result = await _db.ExecuteAsync(sql, new { Id = role.Id, ConcurrencyStamp = role.ConcurrencyStamp });
            return result == 0 ? IdentityResult.Failed(new IdentityError
            {
                Description = "Role deletion failed.",
                Code = "RoleDeletionFailed"
            }) : IdentityResult.Success;
        }


        public async Task<IdentityRole<int>?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(roleId)) throw new ArgumentNullException(nameof(roleId));
            var sql = "Select * from AspNetRoles where Id = @Id;";
            return await _db.QuerySingleOrDefaultAsync<IdentityRole<int>>(sql, new { Id = int.Parse(roleId) });
        }

        public async Task<IdentityRole<int>?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));
            var sql = "Select * from AspNetRoles where NormalizedName = @NormalizedName;";
            return await _db.QuerySingleOrDefaultAsync<IdentityRole<int>>(sql, new { NormalizedName = normalizedRoleName });
        }

        public void Dispose()
        {
            // Dispose of the database connection if necessary
            if (_db is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }




        // These methods don't interact with the database directly.
        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string?> GetRoleNameAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole<int> role, string? normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            role.NormalizedName = normalizedName?.ToUpperInvariant();
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole<int> role, string? roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null) throw new ArgumentNullException(nameof(role));
            var sql = "UPDATE AspNetRoles SET Name = @Name, NormalizedName = @NormalizedName WHERE Id = @Id";
            var result = await _db.ExecuteAsync(sql, new
            {
                role.Name,
                role.NormalizedName,
                Id = role.Id
            });
            return result == 0 ? IdentityResult.Failed(new IdentityError
            {
                Description = "Role update failed.",
                Code = "RoleUpdateFailed"
            }) : IdentityResult.Success;
        }
    }
}