using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Car
{
    public class CarCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1886, int.MaxValue)]
        public int Year { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}