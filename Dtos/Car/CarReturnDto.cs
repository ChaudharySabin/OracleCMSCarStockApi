using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Car
{
    public class CarReturnDto
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public int Stock { get; set; }
        public int? DealerId { get; set; }

    }
}