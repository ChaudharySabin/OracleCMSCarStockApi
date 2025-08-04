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
        public Task<IEnumerable<Car>> GetAllCars();
        public Task<Car?> GetCarById(int id);

        public Task<Car> CreateCar(Car car);

        public Task<Car?> UpdateCar(int id, string make, string model, int Year);
        public Task<(Car?, Dealer?)> UpdateCarDealer(int id, int dealerId);

        public Task<Car?> UpdateCarStock(int id, int stock);

        public Task<Car?> RemoveCar(int id);

        public Task<IEnumerable<Car>> SearchByMakeModel(string? make, string? model);
    }
}