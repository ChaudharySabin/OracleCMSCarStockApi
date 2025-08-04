using Microsoft.EntityFrameworkCore;
using api.Data;
// using NSwag.AspNetCore;
using api.Interfaces;
using api.Repository;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using api.Service;
// using NSwag;
// using NSwag.Generation.Processors.Security;

using Microsoft.OpenApi.Models;
using api.Requirements;
using Microsoft.AspNetCore.Authorization;
using api.Handlers;


var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddOpenApi();
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

//DBcontext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLSERVERCONNECTION")));


builder.Services.AddControllers();

//DI
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IDealerRepository, DealerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IAuthorizationHandler, MustBeOwnUserHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MustHaveSameDealerIdHandler>();



builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

    }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
        options.DefaultChallengeScheme =
        options.DefaultForbidScheme =
        options.DefaultScheme =
        options.DefaultSignInScheme =
        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

    }).AddJwtBearer(options =>
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

await SeedUsersAsync(app);
// app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

async Task SeedUsersAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    // Ensure roles exist (in case you haven't seeded them via HasData)
    var roles = new[] { "SuperAdmin", "Dealer" };
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
    }

    // 1) SuperAdmin user
    var superEmail = "superadmin@example.com";
    var super = await userManager.FindByEmailAsync(superEmail);
    if (super == null)
    {
        super = new User
        {
            UserName = "superadmin",
            Name = "SuperAdminExample",
            Phone = "0123456789",
            Email = superEmail,
            EmailConfirmed = true,
            DealerId = null    // SuperAdmin isn’t tied to a dealer
        };
        await userManager.CreateAsync(super, "Password123#");
        await userManager.AddToRoleAsync(super, "SuperAdmin");
    }

    // 2) Dealer user
    var dealerEmail = "dealer@example.com";
    var dealer = await userManager.FindByEmailAsync(dealerEmail);
    if (dealer == null)
    {
        dealer = new User
        {
            UserName = "dealeruser",
            Name = "DealerExampleUser",
            Phone = "0987654321",
            Email = dealerEmail,
            EmailConfirmed = true,
            DealerId = 1   // adjust to your seeded Dealer.Id
        };
        await userManager.CreateAsync(dealer, "Password123#");
        await userManager.AddToRoleAsync(dealer, "Dealer");
    }
}