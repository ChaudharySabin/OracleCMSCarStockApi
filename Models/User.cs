using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class User : IdentityUser<int>
    {
        // public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; } = string.Empty;

        // public string PasswordHash { get; set; } = string.Empty;

        public int? DealerId { get; set; }

        public string? DealerName { get; set; }

        public string? RoleName { get; set; }
    }
}