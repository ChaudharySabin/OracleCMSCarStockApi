using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using api.Models;
using api.Dtos.Car;

namespace api.Mappers
{
    public static class CarMappers
    {
        public static CarReturnDto ToCarReturnDto(this Car car)
        {
            return new CarReturnDto
            {
                Id = car.Id,
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                Stock = car.Stock,
                DealerId = car.DealerId,
                DealerName = car.DealerName
            };
        }


        public static Car ToCarFromCreateDto(this CarCreateDto carCreateDto, int dealerId)
        {
            return new Car
            {
                Make = carCreateDto.Make,
                Model = carCreateDto.Model,
                Year = carCreateDto.Year,
                Stock = carCreateDto.Stock,
                DealerId = dealerId
            };
        }
    }
}