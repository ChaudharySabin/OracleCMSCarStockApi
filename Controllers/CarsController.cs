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
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/cars")]

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
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<IEnumerable<CarReturnDto>>> GetAllCars()
        {
            var cars = await _carRepo.GetAllCars();
            if (User.IsInRole("SuperAdmin"))
            {
                var carDtos = cars.Select(car => car.ToCarReturnDto()).ToList();
                return carDtos;
            }

            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return Forbid("Dealer is not assigned");

            }

            cars = cars.Where(c => c.DealerId == dealerId).ToList();
            return cars.Select(c => c.ToCarReturnDto()).ToList();

        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> GetCarById([FromRoute] int id)
        {
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            var car = await _carRepo.GetCarById(id);
            if (User.IsInRole("SuperAdmin"))
            {
                if (car == null)
                {
                    return NotFound("No Car Record coudn't be found");
                }
                car.ToCarReturnDto();
            }

            if (car == null)
            {
                return Forbid();
            }

            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned");
            }

            if (car.DealerId != dealerId)
            {
                return Forbid();
            }

            return car.ToCarReturnDto();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> CreateCar([FromBody] CarCreateDto carCreateDto)
        {
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned");
            }
            var car = carCreateDto.ToCarFromCreateDto(dealerId);
            var createdCar = await _carRepo.CreateCar(car);
            return CreatedAtAction(nameof(GetCarById), new { id = createdCar.Id }, createdCar.ToCarReturnDto());
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarInfo([FromRoute] int id, [FromBody] CarUpdateDto carUpdateDto)
        {
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return Forbid("Dealer is not assigned");
            }
            var car = await _carRepo.GetCarById(id);

            if (car == null)
            {
                if (User.IsInRole("SuperAdmin"))
                {
                    return NotFound();
                }
                else
                {
                    return Forbid();
                }
            }


            if (car!.DealerId != dealerId)
            {
                return Forbid();
            }

            await _carRepo.UpdateCar(id, carUpdateDto.Make, carUpdateDto.Model, carUpdateDto.Year);

            if (car == null)
            {
                return NotFound();
            }

            return Ok(car.ToCarReturnDto());

        }

        [HttpPut("update-car-dealer/{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateCarDealer([FromRoute] int id, [FromBody] CarDealerUpdateDto dealerUpdateDto)
        {
            var (car, dealer) = await _carRepo.UpdateCarDealer(id, dealerUpdateDto.DealderId);

            if (car == null)
            {
                return NotFound("The Car Record was not found");
            }

            if (dealer == null)
            {
                return NotFound("The dealer doesn't exist");
            }

            return NoContent();
        }

        [HttpPut("updatestock/{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarStock([FromRoute] int id, [FromBody] CarStockUpdateDto stock)
        {
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return Forbid("Dealer is not assigned");
            }

            var car = await _carRepo.GetCarById(id);
            if (car == null)
            {
                if (User.IsInRole("SuperAdmin"))
                {
                    return NotFound();
                }
                else
                {
                    return Forbid();
                }
            }

            if (car.DealerId != dealerId)
            {
                return Forbid();
            }

            await _carRepo.UpdateCarStock(id, stock.Stock);
            return car.ToCarReturnDto();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<IActionResult> RemoveCar([FromRoute] int id)
        {
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return Forbid("Dealer is not assigned");

            }

            var car = await _carRepo.GetCarById(id);

            if (car == null)
            {
                if (User.IsInRole("SuperAdmin"))
                {
                    return NotFound();
                }
                else
                {
                    return Forbid();
                }
            }

            if (car.DealerId != dealerId)
            {
                return Forbid();
            }

            await _carRepo.RemoveCar(id);

            return NoContent();
        }


        [HttpGet("search")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<IEnumerable<CarReturnDto>>> Search([FromQuery] string? make, [FromQuery] string? model)
        {
            var cars = await _carRepo.SearchByMakeModel(make: make, model: model);
            if (User.IsInRole("SuperAdmin"))
            {
                return cars.Select(c => c.ToCarReturnDto()).ToList();
            }

            var dealerValue = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerValue, out int dealerId))
            {
                return Forbid();
            }

            cars = cars.Where(c => c.DealerId == dealerId).ToList();
            return cars.Select(c => c.ToCarReturnDto()).ToList();
        }

    }
}