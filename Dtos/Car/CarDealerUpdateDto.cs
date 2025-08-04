using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Car
{
    public class CarDealerUpdateDto
    {
        [Required(ErrorMessage = "Stock of the car is required.")]
        [Range(1, int.MaxValue)]
        public int DealderId { get; set; }
    }
}