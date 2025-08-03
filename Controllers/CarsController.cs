using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Mappers;
using api.Dtos.Car;

using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetAllCars()
        {
            var cars = await _context.Cars.ToListAsync();

            List<CarReturnDto> carDtos = cars.Select(car => car.ToCarReturnDto()).ToList();

            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCarById([FromRoute] int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return Ok(car.ToCarReturnDto());
        }

        [HttpPost]
        public async Task<ActionResult<CarReturnDto>> CreateCar([FromBody] CarCreateDto carCreateDto)
        {
            var car = carCreateDto.ToCarFromCreateDto();
            _context.Cars.Add(car);

            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetCarById), new { id = car.Id }, car.ToCarReturnDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarInfo([FromRoute] int id, [FromBody] CarUpdateDto carUpdateDto)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return NotFound();
            }


            car.Make = carUpdateDto.Make;
            car.Model = carUpdateDto.Model;
            car.Year = carUpdateDto.Year;

            await _context.SaveChangesAsync();


            return Ok(car.ToCarReturnDto());

        }


        [HttpPut("updatestock/{id}")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarStock([FromRoute] int id, [FromBody] CarStockUpdateDto stock)
        {
            // Console.WriteLine($"Updating stock for car with ID: {id} to {stock.Stock}");
            var car = await _context.Cars.FindAsync(id);
            Console.WriteLine($"Found car: {car?.Make} {car?.Model} with ID: {id}");
            Console.WriteLine(stock.Stock);
            if (car == null)
            {
                return NotFound();
            }


            car.Stock = stock.Stock;

            await _context.SaveChangesAsync();

            return Ok(car.ToCarReturnDto());
        }



    }
}