using Microsoft.EntityFrameworkCore;
using api.Data;
using NSwag.AspNetCore;
using api.Interfaces;
using api.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLSERVERCONNECTION")));
builder.Services.AddControllers();
builder.Services.AddScoped<ICarRepository, CarRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(
        option =>
        {
            option.DocumentPath = "openapi/v1.json";
            option.DocumentTitle = "Car Stock API";
        }
    );

}

// app.UseHttpsRedirection();
app.MapControllers();
app.Run();

