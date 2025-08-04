using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Account
{
    public class ResetPasswordTokenReturnDto
    {
        public string ResetToken { get; set; } = string.Empty;
    }
}