using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Dealer;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using api.Mappers;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/dealers")]
    public class DealerController : ControllerBase
    {
        private readonly IDealerRepository _dealerRepo;

        public DealerController(IDealerRepository dealerRepo)
        {
            _dealerRepo = dealerRepo;
        }

        /// <summary>
        /// Creates a new dealer.
        /// Requires SuperAdmin role.
        /// </summary>
        /// <param name="dealerCreateDto">The dealer creation data.</param>
        /// <returns>The created dealer.</returns>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<Dealer>> CreateDealer([FromBody] DealerCreateDto dealerCreateDto)
        {
            var dealer = dealerCreateDto.DtoToDealer();
            var dealerCreated = await _dealerRepo.CreateDealerAsync(dealer);
            return Ok(dealerCreated);
        }

        /// <summary>
        /// Retrieves all dealers.
        /// Requires SuperAdmin role.
        /// </summary>
        /// <returns>All dealers.</returns>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<IEnumerable<DealerReturnDto>>> GetAllDealers()
        {
            var dealerList = await _dealerRepo.GetAllDealersAsync();
            return dealerList.Select(d => d.DealerToReturnDto()).ToList();
        }

        /// <summary>
        /// Retrieves a dealer by ID.
        /// Requires SuperAdmin or Dealer role.
        /// SameDealerPolicy ensures that a dealer can only access their own data for Dealer role.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer", Policy = "SameDealerPolicy")]
        public async Task<ActionResult<DealerReturnDto?>> GetDealerById([FromRoute] int id)
        {
            var dealer = await _dealerRepo.GetDealerByIdAsync(id);
            if (dealer == null)
            {
                return NotFound();
            }
            return dealer.DealerToReturnDto();
        }

        /// <summary>
        /// Deletes a dealer by ID.
        /// Requires SuperAdmin role.
        /// Deletes the dealer together with all associated cars.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteDealerByID([FromRoute] int id)
        {
            var dealer = await _dealerRepo.DeleteDealerAsync(id);

            if (dealer == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Updates a dealer's information.
        /// Requires SuperAdmin or Dealer role.
        /// SameDealerPolicy ensures that a dealer can only update their own data for Dealer role.
        /// </summary>
        /// <param name="id">The ID of the dealer to update.</param>
        /// <param name="dealerUpdateDto">The updated dealer information.</param>
        /// <returns>The updated dealer.</returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer", Policy = "SameDealerPolicy")]
        public async Task<ActionResult<DealerReturnDto?>> UpdateDealer([FromRoute] int id, DealerUpdateDto dealerUpdateDto)
        {
            var dealer = await _dealerRepo.UpdateDealerAsync(id, dealerUpdateDto.Name, dealerUpdateDto.Description);
            if (dealer == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetDealerById), new { id = dealer.Id }, dealer.DealerToReturnDto());

        }

    }
}