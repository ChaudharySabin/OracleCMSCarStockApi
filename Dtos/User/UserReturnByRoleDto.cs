using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.User
{
    public class UserReturnByRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}