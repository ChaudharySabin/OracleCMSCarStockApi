using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace api.Service
{
    public class DatabaseInitializerHostedService : IHostedService
    {

        private readonly ILogger<DatabaseInitializerHostedService> _logger;
        private readonly IServiceProvider _sp;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;

        public DatabaseInitializerHostedService(
            ILogger<DatabaseInitializerHostedService> logger,
            IServiceProvider sp,
            IHostEnvironment env,
            IConfiguration config)
        {
            _logger = logger;
            _sp = sp;
            _env = env;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Resolve absolute path to DB file the same way we registered IDbConnection
            _logger.LogInformation("Starting database initialization...");
            if (string.IsNullOrEmpty(_config.GetConnectionString("SQLITECONNECTION")))
            {
                _logger.LogError("SQLITECONNECTION is not configured.");
                throw new InvalidOperationException("SQLITECONNECTION must be set in appsettings.json or environment variables.");
            }


            var rawCs = _config.GetConnectionString("SQLITECONNECTION")!;
            var fileName = rawCs.Split('=', 2)[1].Trim(); // e.g. "Data Source=CarStock.db"
            var dbPath = Path.Combine(_env.ContentRootPath, fileName);

            // Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!); // Ensure directory and the database file exist

            // 1) Ensure the SQLite file exists (open will create if missing)
            _logger.LogInformation("Ensuring SQLite DB at {Path}", dbPath);
            await using (var ensureConn = new SqliteConnection($"Data Source={dbPath}"))
            {
                await ensureConn.OpenAsync(cancellationToken);
            }

            // 2) Run CreateSchema.sql (idempotent)
            var sqlFilePath = Path.Combine(_env.ContentRootPath, "CreateSchema.sql");
            if (!File.Exists(sqlFilePath))
            {
                _logger.LogWarning("CreateSchema.sql not found at {Path}. Skipping schema apply.", sqlFilePath);
            }
            else
            {
                _logger.LogInformation("Applying schema from {Path}", sqlFilePath);
                var sql = await File.ReadAllTextAsync(sqlFilePath, cancellationToken);

                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

                // Using transaction to commit only if all statements succeed
                using var tx = db.BeginTransaction();
                try
                {
                    // SQLite accepts multiple statements separated by ';'
                    await db.ExecuteAsync(sql, transaction: tx);
                    tx.Commit();
                    _logger.LogInformation("Schema applied successfully.");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    _logger.LogError(ex, "Failed to apply schema.");
                    throw;
                }
            }

            // 3) (Optional) seed roles/users AFTER schema exists
            await SeedIdentityAsync(cancellationToken);
            await SeedDealersAsync(cancellationToken);
            await SeedCarsAsync(cancellationToken);
        }

        private async Task SeedIdentityAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            using var scope = _sp.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<api.Models.User>>();

            // roles
            foreach (var role in new[] { "SuperAdmin", "Dealer" })
            {
                if (!await roleMgr.RoleExistsAsync(role))
                {
                    var res = await roleMgr.CreateAsync(new IdentityRole<int>(role));
                    if (!res.Succeeded)
                        throw new InvalidOperationException($"Failed to create role '{role}': {string.Join(",", res.Errors.Select(e => e.Description))}");
                }
            }

            // Local Function to ensure a user exists with a specific role
            // users


            await EnsureUserAsync(email: "superadmin@example.com", userName: "superadmin", FullName: "SuperAdmin", role: "SuperAdmin");
            await EnsureUserAsync(email: "dealer@example.com", userName: "dealeruser", FullName: "Dealer", role: "Dealer");

            // If you also want to seed Dealers/Cars here, you can resolve IDbConnection again and run inserts.
        }



        private async Task SeedDealersAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            // Checking if Dealers table is empty
            var dealerCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Dealers");
            if (dealerCount > 0)
            {
                _logger.LogInformation("Dealers table already seeded with {Count} dealers. Skipping seeding.", dealerCount);
                return;
            }

            // Creating sample dealers
            var dealers = new List<api.Models.Dealer>
            {
                new api.Models.Dealer { Name = "Dealer One", Description = "First dealer" },
                new api.Models.Dealer { Name = "Dealer Two", Description = "Second dealer" },
                new api.Models.Dealer { Name = "Dealer Three", Description = "Third dealer" },
                new api.Models.Dealer { Name = "Dealer Four", Description = "Fourth dealer" },
                new api.Models.Dealer { Name = "Dealer Five", Description = "Fifth dealer" },
                new api.Models.Dealer { Name = "Dealer Six", Description = "Sixth dealer" },
                new api.Models.Dealer { Name = "Dealer Seven", Description = "Seventh dealer" },
                new api.Models.Dealer { Name = "Dealer Eight", Description = "Eighth dealer" },
                new api.Models.Dealer { Name = "Dealer Nine", Description = "Ninth dealer" },
                new api.Models.Dealer { Name = "Dealer Ten", Description = "Tenth dealer" },
            };

            int count = 0;
            foreach (var dealer in dealers)
            {
                var ConcurrencyStamp = Guid.NewGuid().ToString();
                count++;
                var sql = "INSERT INTO Dealers (Name, Description, ConcurrencyStamp) VALUES (@Name, @Description, @ConcurrencyStamp); SELECT last_insert_rowid();";
                var id = await db.ExecuteScalarAsync<int>(sql, new
                {
                    dealer.Name,
                    dealer.Description,
                    ConcurrencyStamp
                });
                await EnsureUserAsync(email: $"dealer{count}@example.com", userName: $"dealeruser{count}", FullName: $"Dealer {count}", role: "Dealer");
            }
            _logger.LogInformation("Dealers seeded successfully");
        }


        private async Task SeedCarsAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            // Checking if Cars table is empty
            var carCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Cars");
            if (carCount > 0)
            {
                _logger.LogInformation("Cars table already seeded with {Count} cars. Skipping seeding.", carCount);
                return;
            }

            // Getting all dealers
            var dealers = await db.QueryAsync<api.Models.Dealer>("SELECT * FROM Dealers");
            if (!dealers.Any())
            {
                _logger.LogWarning("No dealers found. Cannot seed cars without dealers.");
                return;
            }

            //Random car details
            var rnd = new Random();
            var makes = new[] { "Toyota", "Honda", "Ford", "BMW", "Audi", "Kia", "Hyundai", "Nissan", "Chevrolet", "Mazda" };
            var models = new[] { "Sedan", "SUV", "Coupe", "Hatch", "Wagon", "Truck", "Van", "Convert", "Hybrid", "Electric" };
            var years = new[] { 2020, 2021, 2022, 2023, 2024 };
            foreach (var dealer in dealers)
            {
                for (int i = 0; i < 10; i++)
                {
                    var ConcurrencyStamp = Guid.NewGuid().ToString();
                    var sql = "INSERT INTO Cars (Make, Model, Year, DealerId, Stock,ConcurrencyStamp) VALUES (@Make, @Model, @Year, @DealerId, @Stock, @ConcurrencyStamp)";
                    await db.ExecuteAsync(sql, new
                    {
                        Make = makes[rnd.Next(makes.Length)],
                        Model = models[rnd.Next(models.Length)],
                        Year = years[rnd.Next(years.Length)],
                        DealerId = dealer.Id,
                        ConcurrencyStamp = ConcurrencyStamp,
                        Stock = rnd.Next(1, 100) // Random stock between 1 and 100
                    });
                }
            }
            _logger.LogInformation("Cars seeded successfully");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task EnsureUserAsync(string email, string userName, string FullName, string role, string Phone = "0000000000", int? dealerId = null)
        {
            using var scope = _sp.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<api.Models.User>>();
            var u = await userMgr.FindByEmailAsync(email);
            if (u != null) return;

            u = new api.Models.User
            {
                Email = email,
                EmailConfirmed = true,
                UserName = userName,
                Name = FullName,
                DealerId = dealerId,
                Phone = Phone,
                PhoneNumber = Phone,
            };

            var create = await userMgr.CreateAsync(u, "Password123#");
            if (!create.Succeeded)
                throw new InvalidOperationException($"Failed to create user '{email}': {string.Join(",", create.Errors.Select(e => e.Description))}");

            var addRole = await userMgr.AddToRoleAsync(u, role);
            if (!addRole.Succeeded)
                throw new InvalidOperationException($"Failed to add role '{role}' to '{email}': {string.Join(",", addRole.Errors.Select(e => e.Description))}");
        }
    }
}