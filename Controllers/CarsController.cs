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


    }
}