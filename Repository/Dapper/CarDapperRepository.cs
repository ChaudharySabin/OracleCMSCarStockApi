using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using api.Interfaces;
using api.Models;
using Dapper;

namespace api.Repository.Dapper
{
    public class CarDapperRepository : ICarRepository
    {
        private readonly IDbConnection _db;

        public CarDapperRepository(IDbConnection db)
        {
            _db = db;
        }


        public async Task<IEnumerable<Car>> GetAllCars()
        {
            var car = await _db.QueryAsync<Car>("Select Id, Make, Model, Year, Stock, DealerId from Cars");
            return car.ToList();
        }

        public async Task<Car?> GetCarById(int id)
        {
            var car = await _db.QuerySingleOrDefaultAsync<Car>(
                "Select Id, Make, Model, Year, Stock, DealerId from Cars where Id= @Id", new { Id = id });
            return car;
        }

        public async Task<Car> CreateCar(Car car)
        {
            var sql = "Insert into Cars ( Make, Model, Year, Stock, DealerId)" +
            "Values (@Make, @Model, @Year, @Stock, @DealerId); Select last_insert_rowid();";
            var id = await _db.ExecuteScalarAsync<int>(sql, new { car.Make, car.Model, car.Year, car.Stock, car.DealerId });
            car.Id = id;
            return car;
        }
        public async Task<Car?> UpdateCar(int id, string make, string model, int Year)
        {

            var sql = "Update Cars set Make = @Make, Model = @Model, Year = @Year where Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, Make = make, Model = model, Year = Year });
            if (affectedRows == 0)
            {
                return null;
            }
            return await GetCarById(id);
        }

        public async Task<(Car?, Dealer?)> UpdateCarDealer(int id, int dealerId)
        {

            var car = await GetCarById(id);
            if (car == null)
            {
                return (null, null);
            }

            var dealer = await _db.QuerySingleOrDefaultAsync<Dealer>("Select * from Dealers where Id = @DealerId", new { DealerId = dealerId });
            if (dealer == null)
            {
                return (car, null);
            }

            var updateSql = "Update Cars set DealerId = @DealerId where Id = @Id";
            var affectedRow = await _db.ExecuteAsync(updateSql, new { DealerId = dealerId, Id = id });
            if (affectedRow == 0)
            {
                return (null, null);
            }
            return (car, dealer);

        }

        public async Task<Car?> UpdateCarStock(int id, int stock)
        {

            var sql = "Update Cars set Stock = @Stock where Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, Stock = stock });
            if (affectedRows == 0)
            {
                return null;
            }

            return await GetCarById(id);
        }

        public async Task<Car?> RemoveCar(int id)
        {
            var sql = "Delete from Cars where Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id });
            if (affectedRows == 0)
            {
                return null;
            }

            return await GetCarById(id);
        }

        public async Task<IEnumerable<Car>> SearchByMakeModel(string? make, string? model)
        {
            var sql = "Select * from Cars where 1=1";
            if (!string.IsNullOrEmpty(make))
            {
                sql += " and Make like @Make";
            }

            if (!string.IsNullOrEmpty(model))
            {
                sql += " and Model like @Model";
            }

            return await _db.QueryAsync<Car>(sql, new { Make = $"%{make}%", Model = $"%{model}%" });
        }
    }
}