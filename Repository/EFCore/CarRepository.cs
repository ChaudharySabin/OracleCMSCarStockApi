using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Car;
using api.Interfaces;
using api.Models;
using api.Mappers;
using api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace api.EFcore.Repository
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;

        public CarRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Car> CreateCarAsync(Car car)
        {
            await _context.Cars.AddAsync(car);

            await _context.SaveChangesAsync();

            return car;
        }

        public async Task<IEnumerable<Car>> GetAllCarsAsync()
        {
            return await _context.Cars.ToListAsync();

        }

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            return await _context.Cars.FindAsync(id);
        }

        public async Task<Car?> GetCarByIdWithDealerAsync(int id, int dealerId)
        {
            return await _context.Cars.FirstOrDefaultAsync(c => c.Id == id && c.DealerId == dealerId);
        }

        public async Task<Car?> RemoveCarAsync(int id)
        {

            Car? car = _context.Cars.Find(id);

            if (car == null)
            {
                return car;
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return car;
        }

        public async Task<IEnumerable<Car>> SearchByMakeModelAsync(string? make, string? model)
        {
            var car = await _context.Cars.ToListAsync();

            IEnumerable<Car> result = car;

            if (!string.IsNullOrWhiteSpace(make))
            {
                result = result.Where(c =>
                    c.Make.Contains(make, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                result = result.Where(c =>
                    c.Model.Contains(model, StringComparison.OrdinalIgnoreCase));
            }

            return result;
        }

        public async Task<Car?> UpdateCarAsync(int id, string make, string model, int year)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return null;
            }

            car.Make = make;
            car.Model = model;
            car.Year = year;

            await _context.SaveChangesAsync();

            return car;

        }

        public async Task<(Car?, Dealer?)> UpdateCarDealerAsync(int id, int dealerId)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return (car, null);
            }

            var dealer = await _context.Dealers.FindAsync(dealerId);
            if (dealer == null)
            {
                return (car, dealer);
            }

            car.DealerId = dealerId;

            await _context.SaveChangesAsync();

            return (car, dealer);
        }

        public async Task<Car?> UpdateCarStockAsync(int id, int stock)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return null;
            }

            car.Stock = stock;

            await _context.SaveChangesAsync();

            return car;
        }
    }
}