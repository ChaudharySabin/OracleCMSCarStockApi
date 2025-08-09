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

        /// <summary>
        /// Retrieves all cars from the database, including dealer names.
        /// </summary>
        /// <returns>
        /// A list of all cars with their dealer names.
        /// </returns>
        public async Task<IEnumerable<Car>> GetAllCarsAsync()
        {
            // Using a left join to include dealer names in the car list
            var car = await _db.QueryAsync<Car>(
                "Select Cars.Id, Cars.DealerId, Cars.Make, Cars.Model, Cars.Year, Cars.Stock, Dealers.Name as DealerName from Cars left join Dealers on Cars.DealerId = Dealers.Id;"
            );
            return car.ToList();
        }


        /// <summary>
        /// Retrieves a car by its ID, including the dealer name.
        /// </summary>
        /// <param name="id">The ID of the car to retrieve.</param>
        /// <returns>
        /// The car with the specified ID, or null if not found.
        /// </returns>
        public async Task<Car?> GetCarByIdAsync(int id)
        {
            // Using a left join to include dealer names in the car details
            var car = await _db.QuerySingleOrDefaultAsync<Car>(
                "Select c.Id, c.DealerId, c.Make, c.Model, c.Year, c.Stock, d.Name as DealerName, c.ConcurrencyStamp " +
                "from Cars as c left join Dealers as d on c.DealerId = d.Id where c.Id= @Id;",
                new { Id = id });
            return car;
        }


        public async Task<Car?> GetCarByIdWithDealerAsync(int id, int dealerId)
        {
            // Using a left join to include dealer names in the car details
            var car = await _db.QuerySingleOrDefaultAsync<Car>(
                "Select c.Id, c.DealerId, c.Make, c.Model, c.Year, c.Stock, d.Name as DealerName, c.ConcurrencyStamp " +
                "from Cars as c left join Dealers as d on c.DealerId = d.Id where c.Id= @Id and c.DealerId = @DealerId;",
                new { Id = id, DealerId = dealerId });
            return car;
        }

        /// <summary>
        /// Creates a new car in the database.
        /// /// </summary>
        /// <param name="car">The car to create.</param>
        /// <returns>
        /// The created car object.
        public async Task<Car> CreateCarAsync(Car car)
        {
            //We have added a ConcurrencyStamp to the Car model to handle concurrency issues similar to how ASP.NET Identity handles concurrency.
            var ConcurrencyStamp = Guid.NewGuid().ToString();
            var sql = "Insert into Cars ( Make, Model, Year, Stock, DealerId, ConcurrencyStamp)" +
            "Values (@Make, @Model, @Year, @Stock, @DealerId, @ConcurrencyStamp); " +
            "Select last_insert_rowid();";

            var id = await _db.ExecuteScalarAsync<int>(sql, new { car.Make, car.Model, car.Year, car.Stock, car.DealerId, ConcurrencyStamp });
            car.Id = id;
            car.ConcurrencyStamp = ConcurrencyStamp;

            return car;
        }

        /// <summary>
        /// Updates an existing car in the database.
        /// /// </summary>
        /// <param name="id">The ID of the car to update.</param>
        /// <param name="make">The new make of the car.</param>
        /// <param name="model">The new model of the car.</param>
        /// <param name="Year">The new year of the car.</param>
        /// <returns>
        /// The updated car object, or null if the update failed.
        /// </returns>
        public async Task<Car?> UpdateCarAsync(int id, string make, string model, int Year)
        {
            // Check if the car exists
            var existingCar = await GetCarByIdAsync(id);
            if (existingCar == null)
            {
                return null; // Car not found
            }

            var oldConcurrencyStamp = existingCar.ConcurrencyStamp;
            String newConcurrencyStamp = Guid.NewGuid().ToString();

            // We are also checking null as ConcurrencyStamp can be null in the database and a simple ConcurrencyStamp = null will never be true 
            var sql = "Update Cars set Make = @Make, Model = @Model, Year = @Year, ConcurrencyStamp = @newConcurrencyStamp where Id = @Id and (ConcurrencyStamp = @oldConcurrencyStamp or ConcurrencyStamp is null); ";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, Make = make, Model = model, Year = Year, newConcurrencyStamp, oldConcurrencyStamp });

            // The ExecuteAsync method returns the number of rows affected by the update operation.
            if (affectedRows == 0)
            {
                // If no rows were affected, it means the update failed, possibly due to concurrency issues.
                throw new Exception("Something went wrong while updating the car.");
            }
            // Commit the transaction if the update was successful

            return await GetCarByIdAsync(id);
        }


        /// <summary>
        /// Updates the dealer of a car by its ID.
        /// </summary>
        /// <param name="id">The ID of the car to update.</param>
        /// <param name="dealerId">The ID of the dealer to associate with the car.</param>
        /// <returns>
        /// A tuple containing the updated car object and the associated dealer object, or null if the car or dealer was not found.
        /// </returns>
        public async Task<(Car?, Dealer?)> UpdateCarDealerAsync(int id, int dealerId)
        {

            var car = await GetCarByIdAsync(id);
            if (car == null)
            {
                return (null, null);
            }

            var dealer = await _db.QuerySingleOrDefaultAsync<Dealer>("Select * from Dealers where Id = @DealerId;", new { DealerId = dealerId });
            if (dealer == null)
            {
                return (car, null);
            }
            // Update the car's dealer ID
            string? oldConcurrencyStamp = car.ConcurrencyStamp;
            Console.WriteLine($"Old Concurrency Stamp: {oldConcurrencyStamp}");
            string newConcurrencyStamp = Guid.NewGuid().ToString();
            // We are also checking null as ConcurrencyStamp can be null in the database and a simple ConcurrencyStamp = null will never be true 
            var updateSql = "Update Cars set DealerId = @DealerId, ConcurrencyStamp = @NewConcurrencyStamp where Id = @Id AND (ConcurrencyStamp = @OldConcurrencyStamp OR ConcurrencyStamp IS NULL);";
            var affectedRow = await _db.ExecuteAsync(
                updateSql,
                new { DealerId = dealerId, NewConcurrencyStamp = newConcurrencyStamp, Id = id, OldConcurrencyStamp = oldConcurrencyStamp }
            );

            //ExecuteAsync returns the number of rows affected by the update operation.
            if (affectedRow == 0)
            {
                // If no rows were affected, it means the update failed, possibly due to concurrency issues.
                throw new Exception("Something went wrong while updating the car's dealer.");
            }
            return (car, dealer);

        }

        /// <summary>
        /// Updates the stock of a car by its ID.
        /// </summary>
        /// <param name="id">The ID of the car to update.</param>
        /// <param name="stock">The new stock value for the car.</param>
        /// <returns>
        /// The updated car object with the new stock value, or null if the car was not found.
        /// </returns>
        public async Task<Car?> UpdateCarStockAsync(int id, int stock)
        {
            // Check if the car exists
            var existingCar = await GetCarByIdAsync(id);
            if (existingCar == null)
            {
                return null; // Car not found
            }
            // Update the stock of the car
            string? oldConcurrencyStamp = existingCar.ConcurrencyStamp;
            string newConcurrencyStamp = Guid.NewGuid().ToString();
            // We are also checking null as ConcurrencyStamp can be null in the database and a simple ConcurrencyStamp = null will never be true 
            var sql = "Update Cars set Stock = @Stock, ConcurrencyStamp = @newConcurrencyStamp where Id = @Id and (ConcurrencyStamp = @oldConcurrencyStamp or ConcurrencyStamp is null); ";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, Stock = stock, newConcurrencyStamp, oldConcurrencyStamp });

            if (affectedRows == 0)
            {
                throw new Exception("Something went wrong while updating the car's stock.");
            }

            // Returning the updated car object with the new stock value and concurrency stamp
            // This is returned instead of using the GetCarById method to avoid an additional database call.
            existingCar.Stock = stock;
            existingCar.ConcurrencyStamp = newConcurrencyStamp;
            return existingCar;
        }

        /// <summary>
        /// Removes a car from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the car to remove.</param>
        /// <returns>
        /// The removed car object if successful, or null if the car was not found.
        /// </returns>
        public async Task<Car?> RemoveCarAsync(int id)
        {
            // Check if the car exists
            var existingCar = await GetCarByIdAsync(id);
            if (existingCar == null)
            {
                return null; // Car not found
            }
            string? oldConcurrencyStamp = existingCar.ConcurrencyStamp;
            // We are also checking null as ConcurrencyStamp can be null in the database and a simple ConcurrencyStamp = null will never be true 
            var sql = "Delete from Cars where Id = @Id and (ConcurrencyStamp = @oldConcurrencyStamp or ConcurrencyStamp is null); ";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id, oldConcurrencyStamp = oldConcurrencyStamp });

            if (affectedRows == 0)
            {
                throw new Exception("Something went wrong while deleting the car.");
            }

            return await GetCarByIdAsync(id);
        }

        /// <summary>
        /// Searches for cars by make and model.
        /// </summary>
        /// <param name="make">The make of the car to search for.</param>
        /// <param name="model">The model of the car to search for.</param>
        /// <returns>
        /// An enumerable collection of cars that match the search criteria.
        /// </returns>
        public async Task<IEnumerable<Car>> SearchByMakeModelAsync(string? make, string? model)
        {
            // The 1=1 conditions is used here for easiy chaining of and conditions in the SQL query.
            var sql = "Select * from Cars where 1=1";
            if (!string.IsNullOrEmpty(make))
            {
                sql += " and Make like @Make";
            }

            if (!string.IsNullOrEmpty(model))
            {
                sql += " and Model like @Model";
            }

            var result = await _db.QueryAsync<Car>(sql, new { Make = $"%{make}%", Model = $"%{model}%" });
            return result.ToList();
        }


    }
}