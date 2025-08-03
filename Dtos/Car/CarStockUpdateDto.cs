using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Car
{
    public class CarStockUpdateDto
    {
        [Required(ErrorMessage = "Stock of the car is required.")]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}