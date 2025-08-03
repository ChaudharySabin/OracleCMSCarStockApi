using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Dealer
{
    public class DealerUpdateDto
    {
        [Required(ErrorMessage = "Name is Required")]
        [StringLength(100), MinLength(1)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}