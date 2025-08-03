using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using api.Dtos.User;
using api.Mappers;
using api.QueryHelpers;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReturnDto>>> GetAllUsers()
        {
            var allUsers = await _userRepo.GetAllUsersAsync();

            return allUsers.Select(u => u.UserToReturnDto()).ToList();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserReturnDto>> GetUserById([FromRoute] int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user.UserToReturnDto();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserReturnDto>> UpdateUserInfo(int id, UserUpdateInfoDto userUpdateInfoDto)
        {
            var user = await _userRepo.UpdateUserAsync(id, userUpdateInfoDto.Name, userUpdateInfoDto.Email, userUpdateInfoDto.Phone);
            if (user == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user.UserToReturnDto());
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateUserDealer([FromRoute] int id, [FromQuery] UserDealerUpdateQueryObject userDealerUpdateQueryObject)
        {
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest();
            // }

            // Console.WriteLine(userDealerUpdateQueryObject.DealerId);
            // return Ok();
            var (user, dealer) = await _userRepo.UpdateUserDealerIdAsync(id, userDealerUpdateQueryObject.DealerId);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            if (user != null && dealer == null)
            {
                return NotFound("Dealer With the provided dealerId doesnot exists");
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user!.Id }, user.UserToReturnDto());

        }


        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            var deletedUser = await _userRepo.DeleteUserAsync(id);
            if (deletedUser == null)
            {
                return NotFound("User Not Found");
            }

            return NoContent();
        }



    }
}