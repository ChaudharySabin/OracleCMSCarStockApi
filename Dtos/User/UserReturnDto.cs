using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Dtos.User
{
    public class UserReturnDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public int? DealerId { get; set; }

        public string? DealerName { get; set; }

        public string? RoleName { get; set; }
    }
}