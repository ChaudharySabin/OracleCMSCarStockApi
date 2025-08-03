using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Car
{
    public class CarCreateDto
    {
        [Required(ErrorMessage = "Make of the car is required.")]
        [StringLength(100), MinLength(1)]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model of the car is required.")]
        [StringLength(100), MinLength(1)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year of the car is required.")]
        [Range(1886, int.MaxValue)]
        public int Year { get; set; }

        [Required(ErrorMessage = "Stock of the car is required.")]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}