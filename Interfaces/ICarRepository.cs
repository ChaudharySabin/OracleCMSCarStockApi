using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Dtos.Car;

namespace api.Interfaces
{
    public interface ICarRepository
    {
        public Task<IEnumerable<Car>> GetAllCarsAsync();
        public Task<Car?> GetCarByIdAsync(int id);

        public Task<Car?> GetCarByIdWithDealerAsync(int id, int dealerId);

        public Task<Car> CreateCarAsync(Car car);

        public Task<Car?> UpdateCarAsync(int id, string make, string model, int Year);
        public Task<(Car?, Dealer?)> UpdateCarDealerAsync(int id, int dealerId);

        public Task<Car?> UpdateCarStockAsync(int id, int stock);

        public Task<Car?> RemoveCarAsync(int id);

        public Task<IEnumerable<Car>> SearchByMakeModelAsync(string? make, string? model);
    }
}