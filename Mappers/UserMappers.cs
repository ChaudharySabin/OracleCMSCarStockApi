using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.User;
using api.Models;

namespace api.Mappers
{
    public static class UserMappers
    {
        public static UserReturnDto UserToReturnDto(this User user)
        {
            return new UserReturnDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                DealerId = user.DealerId,
                Name = user.Name,
                Email = user.Email!,
                Phone = user.Phone ?? string.Empty,
                DealerName = user.DealerName,
                RoleName = user.RoleName ?? string.Empty
            };
        }
    }
}