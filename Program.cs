using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using api.Service;
using Microsoft.OpenApi.Models;
using api.Requirements;
using Microsoft.AspNetCore.Authorization;
using api.Handlers;
using api.Configuration;
using System.Data;
using Microsoft.Data.Sqlite;
using api.EFcore.Repository;
using api.Repository.Dapper;
using api.Stores;



var builder = WebApplication.CreateBuilder(args);


//Adding Swagger UI with Authorize button
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
// Till Here

//DBcontext
//SQLSERVERConnection
// builder.Services.AddDbContext<ApplicationDbContext>(o =>
//     o.UseSqlServer(builder.Configuration.GetConnectionString("SQLSERVERCONNECTION")));

//SqlLite Connection
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
// options.UseSqlite(builder.Configuration.GetConnectionString("SQLITECONNECTION")));


//In-Memory Database Connection
// builder.Services.AddDbContext<ApplicationDbContext>(o =>
//         o.UseInMemoryDatabase("CarStockInMemDb"));


//Transient Connection For Dapper
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connection = new SqliteConnection(builder.Configuration.GetConnectionString("SQLITECONNECTION"));
    connection.Open();
    return connection;
});

builder.Services.AddControllers();

//DI
builder.Services.AddScoped<ICarRepository, CarDapperRepository>();
builder.Services.AddScoped<IDealerRepository, DealerDapperRepository>();
builder.Services.AddScoped<IUserRepository, UserDapperRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IAuthorizationHandler, MustBeOwnUserHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MustHaveSameDealerIdHandler>();


//Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultPhoneProvider;

    })
    // .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddUserStore<UserDapperStore>()
    .AddRoleStore<RoleDapperStore>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme =
     options.DefaultChallengeScheme =
     options.DefaultForbidScheme =
     options.DefaultScheme =
     options.DefaultSignInScheme =
     options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
    }
)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)
                                )
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnUserPolicy", policy =>
    {
        policy.Requirements.Add(new MustBeOwnUserRequirement());
    });

    options.AddPolicy("SameDealerPolicy", policy =>
    {
        policy.Requirements.Add(new MustHaveSameDealerIdRequirement());
    });
});


//Email
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}


// await SeedDataAsync(app); //This can be removed later on and is only there to seed some data on in memory startup

app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();


// Seed data
// async Task SeedDataAsync(WebApplication app)
// {
//     using var scope = app.Services.CreateScope();
//     var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
//     var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

//     ctx.Database.EnsureCreated();

//     // Ensure roles exist
//     var roles = new[] { "SuperAdmin", "Dealer" };
//     foreach (var roleName in roles)
//     {
//         if (!await roleManager.RoleExistsAsync(roleName))
//             await roleManager.CreateAsync(new IdentityRole<int>(roleName));
//     }

//     //This will be seeded both in In Memory and In Database if not found already
//     var superEmail = "superadmin@example.com";
//     var super = await userManager.FindByEmailAsync(superEmail);
//     if (super == null)
//     {
//         super = new User
//         {
//             UserName = "superadmin",
//             Name = "SuperAdminExample",
//             Phone = "0123456789",
//             Email = superEmail,
//             EmailConfirmed = true,
//             DealerId = null    // SuperAdmin isn’t tied to a dealer
//         };
//         await userManager.CreateAsync(super, "Password123#");
//         await userManager.AddToRoleAsync(super, "SuperAdmin");
//     }
//     var dealeruser = "dealer@example.com";
//     var dealer = await userManager.FindByEmailAsync(dealeruser);
//     if (dealer == null)
//     {
//         dealer = new User
//         {
//             UserName = "dealeruser",
//             Name = "Dealeruserexample",
//             Phone = "0123456789",
//             Email = dealeruser,
//             EmailConfirmed = true,
//             DealerId = null    // SuperAdmin isn’t tied to a dealer
//         };
//         await userManager.CreateAsync(dealer, "Password123#");
//         await userManager.AddToRoleAsync(dealer, "Dealer");
//     }
//     //Uptill here


//     // if (useInMemory)
//     // {
//     // 4) Seed 10 Dealers
//     if (!ctx.Dealers.Any())
//     {
//         var dealers = Enumerable.Range(1, 10)
//             .Select(i => new Dealer
//             {
//                 Name = $"Dealer {i}",
//                 Description = $"Description for Dealer {i}"
//             })
//             .ToList();

//         ctx.Dealers.AddRange(dealers);
//         ctx.SaveChanges();


//         // 5) For each Dealer, seed one Dealer‐role user
//         var allDealers = ctx.Dealers.ToList();
//         foreach (var createdDealer in allDealers)
//         {
//             var email = $"dealer{createdDealer.Id}@example.com";
//             if (await userManager.FindByEmailAsync(email) == null)
//             {
//                 var dealerUser = new User
//                 {
//                     UserName = $"dealer{createdDealer.Id}",
//                     Email = email,
//                     EmailConfirmed = true,
//                     DealerId = createdDealer.Id,
//                     Name = $"Dealer User {createdDealer.Id}",
//                     Phone = $"555-000{createdDealer.Id:00}"
//                 };
//                 await userManager.CreateAsync(dealerUser, "Password123#");
//                 await userManager.AddToRoleAsync(dealerUser, "Dealer");
//             }
//         }
//     }

// 6) Seed one Car per Dealer (10 Cars total)
// if (!ctx.Cars.Any())
// {
//     var allDealers = ctx.Dealers.ToList();
//     var rnd = new Random();
//     var makes = new[] { "Toyota", "Honda", "Ford", "BMW", "Audi", "Kia", "Hyundai", "Nissan", "Chevrolet", "Mazda" };
//     var models = new[] { "Sedan", "SUV", "Coupe", "Hatch", "Wagon", "Truck", "Van", "Convert", "Hybrid", "Electric" };

//     var cars = allDealers
//     .SelectMany(dealer =>
//         Enumerable.Range(1, 10).Select(i => new Car
//         {
//             Make = makes[rnd.Next(makes.Length)],
//             Model = models[rnd.Next(models.Length)],
//             Year = rnd.Next(2000, DateTime.Now.Year + 1),
//             Stock = rnd.Next(1, 100),
//             DealerId = dealer.Id
//         })
//     )
//     .ToList();

//     ctx.Cars.AddRange(cars);
//     ctx.SaveChanges();
// }
// }



//Additional Codes that were commented
// builder.Services.AddOpenApi();
//Can Be Removed from here will do later
// var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
// if (useInMemory)
// {
//     builder.Services.AddDbContext<ApplicationDbContext>(o =>
//         o.UseInMemoryDatabase("CarStockInMemDb"));
// }
// else
// {
// builder.Services.AddDbContext<ApplicationDbContext>(o =>
//     o.UseSqlServer(builder.Configuration.GetConnectionString("SQLSERVERCONNECTION")));
// }
//TIll here