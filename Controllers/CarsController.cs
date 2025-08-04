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
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/cars")]
    [Authorize]
    public class CarsController : ControllerBase
    {
        // private readonly ApplicationDbContext _context;
        private readonly ICarRepository _carRepo;

        public CarsController(ICarRepository carRepo)
        {
            // _context = context;
            _carRepo = carRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetAllCars()
        {
            var cars = await _carRepo.GetAllCars();

            List<CarReturnDto> carDtos = cars.Select(car => car.ToCarReturnDto()).ToList();

            return Ok(cars);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Car>> GetCarById([FromRoute] int id)
        {
            var car = await _carRepo.GetCarById(id);
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
            var createdCar = await _carRepo.CreateCar(car);
            return CreatedAtAction(nameof(GetCarById), new { id = createdCar.Id }, createdCar.ToCarReturnDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarInfo([FromRoute] int id, [FromBody] CarUpdateDto carUpdateDto)
        {
            var car = await _carRepo.UpdateCar(id, carUpdateDto.Make, carUpdateDto.Model, carUpdateDto.Year);

            if (car == null)
            {
                return NotFound();
            }

            return Ok(car.ToCarReturnDto());

        }


        [HttpPut("updatestock/{id:int}")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarStock([FromRoute] int id, [FromBody] CarStockUpdateDto stock)
        {
            // Console.WriteLine($"Updating stock for car with ID: {id} to {stock.Stock}");
            var car = await _carRepo.UpdateCarStock(id, stock.Stock);

            if (car == null)
            {
                return NotFound();
            }

            return Ok(car.ToCarReturnDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveCar([FromRoute] int id)
        {
            var car = await _carRepo.RemoveCar(id);

            if (!car)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CarReturnDto>>> Search([FromQuery] string? make, [FromQuery] string? model)
        {
            var cars = await _carRepo.SearchByMakeModel(make: make, model: model);

            return Ok(cars.Select(c => c.ToCarReturnDto()));

        }
    }
}