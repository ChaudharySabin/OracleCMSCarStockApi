using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class UserUpdateInfoDto
    {
        [Required]
        [StringLength(100), MinLength(1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100), MinLength(1)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100), MinLength(1)]
        public string Phone { get; set; } = string.Empty;
    }
}