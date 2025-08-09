using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Models;

namespace api.Mappers
{
    public static class AuthenticatedUserMapper
    {
        public static AuthenticatedUserReturnDto AuthUserReturnDto(this User user, string token)
        {
            return new AuthenticatedUserReturnDto
            {
                Id = user.Id,
                DealderId = user.DealerId,
                Name = user.Name,
                Email = user.Email!,
                Phone = user.Phone ?? string.Empty,
                Token = token,
                // Dealer = user.Dealer
            };
        }
    }
}