using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Dealer;
using api.Models;

namespace api.Mappers
{
    public static class DealerMappers
    {
        public static Dealer DtoToDealer(this DealerCreateDto dealerCreateDto)
        {
            return new Dealer
            {
                Name = dealerCreateDto.Name,
                Description = dealerCreateDto.Description
            };
        }

        public static DealerReturnDto DealerToReturnDto(this Dealer dealer)
        {
             return new DealerReturnDto
            {
                Id = dealer.Id,
                Name = dealer.Name,
                Description = dealer.Description
            };
        }
    }
}