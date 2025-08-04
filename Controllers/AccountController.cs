using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using api.Mappers;

namespace api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                User user = new()
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    Phone = registerDto.Phone,
                    PhoneNumber = registerDto.Phone,
                    Name = registerDto.Name
                };

                var createdUser = await _userManager.CreateAsync(user, registerDto.Password);

                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "DEALER");
                    if (roleResult.Succeeded)
                    {
                        return Ok(
                            new CreatedUserReturnDto
                            {
                                Name = user.Name,
                                Email = user.Email,
                                Token = await _tokenService.CreateToken(user)
                            });
                    }
                    else
                    {
                        return BadRequest(roleResult.Errors);
                    }
                }
                else
                {
                    return BadRequest(createdUser.Errors);
                }

            }
            catch (System.Exception e)
            {

                return StatusCode(500, e.Message);
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthenticatedUserReturnDto>> Login([FromBody] LoginDto loginDto)
        {
            // Lookup the user first
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var validUser = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!validUser)
            {
                return Unauthorized("Invalid email or password.");
            }

            string jwtToken = await _tokenService.CreateToken(user);

            return Ok(user.AuthUserReturnDto(jwtToken));
        }


        [HttpPost("generate-reset-password-token")]
        public async Task<ActionResult<ResetPasswordTokenReturnDto>> GenerateResetPasswordToken([FromBody] GeneratedResetPasswordDto generatedResetPasswordDto)
        {
            // Console.WriteLine(generatedResetPasswordDto.Email);
            var user = await _userManager.FindByEmailAsync(generatedResetPasswordDto.Email.Trim());
            // Console.WriteLine(user);
            if (user == null)
            {
                return NotFound("The user with provided email wasnot found");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Ok(new ResetPasswordTokenReturnDto { ResetToken = token });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email.ToLower());
            if (user == null)
            {
                return NotFound("The user with the provided email was not found");
            }

            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!resetPasswordResult.Succeeded)
                return BadRequest(resetPasswordResult.Errors);

            return NoContent();
        }


    }
}