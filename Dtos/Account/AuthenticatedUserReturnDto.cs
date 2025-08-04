using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Dtos.Account
{
    public class AuthenticatedUserReturnDto
    {
        public int Id { get; set; }

        public int? DealderId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        // public api.Models.Dealer? Dealer { get; set; }


    }
}