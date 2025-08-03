using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Dealer
{
    public class DealerCreateDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100), MinLength(1)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}