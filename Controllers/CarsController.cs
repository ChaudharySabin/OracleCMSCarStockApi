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

        /// <summary>
        /// Retrieves all cars from the database.
        /// If the user is a SuperAdmin, they can access all cars.
        /// If the user is a Dealer, they can only access cars associated with their dealer ID.
        /// </summary>
        /// <returns>
        /// A list of car objects, each represented as a CarReturnDto.

        /// </returns>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<IEnumerable<CarReturnDto>>> GetAllCars()
        {
            var cars = await _carRepo.GetAllCarsAsync();
            if (User.IsInRole("SuperAdmin"))
            {
                var carDtos = cars.Select(car => car.ToCarReturnDto()).ToList();
                return carDtos;
            }

            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");

            }

            cars = cars.Where(c => c.DealerId == dealerId).ToList();
            return cars.Select(c => c.ToCarReturnDto()).ToList();

        }

        /// <summary>
        /// Retrieves a car by its ID.
        /// If the user is a SuperAdmin, they can access any car.
        /// If the user is a Dealer, they can only access cars associated with their dealer ID.
        /// </summary>
        /// <param name="id">The ID of the car to retrieve.</param>
        /// <returns>
        /// A car object represented as a CarReturnDto if found, or a NotFound result if not found.
        /// </returns>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> GetCarById([FromRoute] int id)
        {
            // If the user is a SuperAdmin, they can access any car
            if (User.IsInRole("SuperAdmin"))
            {
                var car = await _carRepo.GetCarByIdAsync(id);
                if (car == null)
                {
                    return NotFound();
                }

            }

            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");
            }
            var carFiltered = await _carRepo.GetCarByIdWithDealerAsync(id, dealerId);
            // If the user is a Dealer, they can only access cars associated with their dealer ID

            //For simiplicity, we can check car == null || car.DealerId != dealerId
            // but we will return NotFound if the car is not found, and Forbid if the dealerId does not match
            // This way, dealers can differentiate between a car not found associated and a dealer not having access to the car
            if (carFiltered == null)
            {
                return NotFound();
            }

            return carFiltered.ToCarReturnDto();
        }

        /// <summary>
        /// Creates a new car.
        /// The user must have a dealer Id to create a car. 
        /// If superAdmin also requires a dealerId to create a car which they can change later to the actual dealer.
        /// </summary>
        /// <param name="carCreateDto">The car data to create.</param>
        /// <returns>
        /// A CarReturnDto representing the created car.
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> CreateCar([FromBody] CarCreateDto carCreateDto)
        {
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");
            }
            var car = carCreateDto.ToCarFromCreateDto(dealerId);
            var createdCar = await _carRepo.CreateCarAsync(car);
            return CreatedAtAction(nameof(GetCarById), new { id = createdCar.Id }, createdCar.ToCarReturnDto());
        }

        /// <summary>
        /// Updates the information of an existing car.
        /// SuperAdmin don't require a dealerId to update the car information.
        /// If the user is a Dealer, they can only update cars associated with their dealer ID.
        /// </summary>
        /// <param name="id">The ID of the car to update.</param>
        /// <param name="carUpdateDto">The updated car data.</param> 
        /// <returns>
        /// A CarReturnDto representing the updated car if successful, or a NotFound result if the car was not found.
        /// </returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarInfo([FromRoute] int id, [FromBody] CarUpdateDto carUpdateDto)
        {

            // If the user is a SuperAdmin, they can update any car without a dealer ID
            if (User.IsInRole("SuperAdmin"))
            {
                var carFullAccess = await _carRepo.GetCarByIdAsync(id);
                if (carFullAccess == null)
                {
                    return NotFound();
                }
                var udpatedCar = await _carRepo.UpdateCarAsync(id, carUpdateDto.Make, carUpdateDto.Model, carUpdateDto.Year);
                if (udpatedCar == null)
                {
                    return NotFound();
                }
                return udpatedCar.ToCarReturnDto();
            }

            // Check if the user is a Dealer and retrieve their dealer ID
            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");
            }
            var car = await _carRepo.GetCarByIdWithDealerAsync(id, dealerId);
            // If the user is a Dealer, they can only update cars associated with their dealer ID
            if (car == null)
            {
                return NotFound();
            }
            if (car.DealerId != dealerId)
            {
                return Forbid();
            }
            var updatedCar = await _carRepo.UpdateCarAsync(id, carUpdateDto.Make, carUpdateDto.Model, carUpdateDto.Year);
            if (updatedCar == null)
            {
                return NotFound();
            }
            return updatedCar.ToCarReturnDto();
        }

        /// <summary>
        /// Updates the dealer associated with a car.
        /// Only SuperAdmin can update the dealer of any car.
        /// </summary>
        /// <param name="id">The ID of the car to update.</param>
        /// <param name="dealerUpdateDto">The updated dealer information.</param>
        /// <returns>
        /// A NoContent result if the update is successful, or a NotFound result if the car or dealer was not found.
        /// </returns>
        [HttpPut("update-car-dealer/{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateCarDealer([FromRoute] int id, [FromBody] CarDealerUpdateDto dealerUpdateDto) //CarDealerUpdateDto: to apply required annotation. Can also be done by embeding in to the route
        {

            var (car, dealer) = await _carRepo.UpdateCarDealerAsync(id, dealerUpdateDto.DealderId);
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

        /// <summary>
        /// Updates the stock of a car.
        /// SuperAdmin can update the stock of any car.
        /// If the user is a Dealer, they can only update the stock of cars associated with their dealer ID.
        /// </summary>
        /// <param name="id">The ID of the car to update.</param>
        /// <param name="stock">The new stock value for the car.</param>
        /// <returns>
        /// A CarReturnDto representing the updated car if successful, or a NotFound result if the car was not found.
        /// </returns>
        [HttpPut("updatestock/{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<CarReturnDto>> UpdateCarStock([FromRoute] int id, [FromBody] CarStockUpdateDto stock)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                var car = await _carRepo.GetCarByIdAsync(id);
                if (car == null)
                {
                    return NotFound();
                }
                var updadedCar = await _carRepo.UpdateCarStockAsync(id, stock.Stock);
                if (updadedCar == null)
                {
                    return NotFound();
                }
                return updadedCar.ToCarReturnDto();
            }


            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");
            }

            var carFilteredWithDealer = await _carRepo.GetCarByIdWithDealerAsync(id, dealerId);
            if (carFilteredWithDealer == null)
            {

                return NotFound();

            }

            var updatedCar = await _carRepo.UpdateCarStockAsync(id, stock.Stock);
            if (updatedCar == null)
            {
                return NotFound();
            }

            return updatedCar.ToCarReturnDto();
        }

        /// <summary>
        /// Removes a car from the database.
        /// Only SuperAdmin can remove any car.
        /// If the user is a Dealer, they can only remove cars associated with their dealer ID.
        /// </summary>
        /// <param name="id">The ID of the car to remove.</param>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<IActionResult> RemoveCar([FromRoute] int id)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                var car = await _carRepo.GetCarByIdAsync(id);
                if (car == null)
                {
                    return NotFound();
                }
                await _carRepo.RemoveCarAsync(id);
                return NoContent();
            }


            var dealerIdClaim = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerIdClaim, out var dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");

            }
            var carfiltered = await _carRepo.GetCarByIdWithDealerAsync(id, dealerId);
            if (carfiltered == null)
            {
                return NotFound();
            }

            await _carRepo.RemoveCarAsync(id);
            return NoContent();
        }


        [HttpGet("search")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
        public async Task<ActionResult<IEnumerable<CarReturnDto>>> Search([FromQuery] string? make, [FromQuery] string? model)
        {
            var cars = await _carRepo.SearchByMakeModelAsync(make: make, model: model);
            if (User.IsInRole("SuperAdmin"))
            {
                return cars.Select(c => c.ToCarReturnDto()).ToList();
            }

            var dealerValue = User.FindFirst("dealerId")?.Value;
            if (!int.TryParse(dealerValue, out int dealerId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Dealer is not assigned, Please contact admin");
            }

            cars = cars.Where(c => c.DealerId == dealerId).ToList();
            return cars.Select(c => c.ToCarReturnDto()).ToList();
        }

    }
}