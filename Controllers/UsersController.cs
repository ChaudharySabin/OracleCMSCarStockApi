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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        public UsersController(IUserRepository userRepo, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<IEnumerable<UserReturnDto>>> GetAllUsers()
        {
            var allUsers = await _userRepo.GetAllUsersAsync();

            return allUsers.Select(u => u.UserToReturnDto()).ToList();
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Dealer")]
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
        [Authorize(Roles = "SuperAdmin,Dealer")]
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
        [Authorize(Roles = "SuperAdmin")]
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
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            var deletedUser = await _userRepo.DeleteUserAsync(id);
            if (deletedUser == null)
            {
                return NotFound("User Not Found");
            }

            return NoContent();
        }


        [HttpGet("{id:int}/role")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<List<string>>> GetRoleByIdForUser([FromRoute] int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound($"The user with the Id {id} doesn't exist");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            return Ok(currentRoles.ToList());

        }


        [HttpPut("{id:int}/update-role")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateUserRole([FromRoute] int id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound($"The user with the Id {id} doesn't exist");
            }

            var role = await _roleManager.RoleExistsAsync(updateRoleDto.Role);
            if (!role)
            {
                return NotFound($"The Role:{updateRoleDto.Role} doesn't exist");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return StatusCode(500, removeResult.Errors);
                }
            }

            var addRolesResult = await _userManager.AddToRoleAsync(user, updateRoleDto.Role);
            if (!addRolesResult.Succeeded)
            {
                return StatusCode(500, addRolesResult.Errors);
            }

            return NoContent();

        }
    }
}