using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class UpdateRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}