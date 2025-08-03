using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.QueryHelpers
{
    public class UserDealerUpdateQueryObject
    {
        [Required]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Please provide a valid DealerId")]
        public int DealerId { get; set; }
    }
}