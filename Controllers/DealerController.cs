using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Dealer;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using api.Mappers;

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

        [HttpPost]
        public async Task<ActionResult<Dealer>> CreateDealer([FromBody] DealerCreateDto dealerCreateDto)
        {
            var dealer = dealerCreateDto.DtoToDealer();
            var dealerCreated = await _dealerRepo.CreateDealerAsync(dealer);
            return Ok(dealerCreated);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<DealerReturnDto>>> GetAllDealers()
        {
            var dealerList = await _dealerRepo.GetAllDealersAsync();
            return dealerList.Select(d => d.DealerToReturnDto()).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DealerReturnDto?>> GetDealerById([FromRoute] int id)
        {
            var dealer = await _dealerRepo.GetDealerByIdAsync(id);
            if (dealer == null)
            {
                return NotFound();
            }
            return dealer.DealerToReturnDto();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDealerByID([FromRoute] int id)
        {
            var dealer = await _dealerRepo.DeleteDealerAsync(id);

            if (dealer == null)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<DealerReturnDto?>> UpdateDealer([FromRoute] int id, DealerUpdateDto dealerUpdateDto)
        {
            var dealer = await _dealerRepo.UpdateDealerAsync(id, dealerUpdateDto.Name, dealerUpdateDto.Description);
            if (dealer == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetDealerById), new {id = dealer.Id}, dealer.DealerToReturnDto());

        }

    }
}