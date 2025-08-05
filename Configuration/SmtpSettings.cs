using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Configuration
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public string From { get; set; } = "noreply@localhost";
    }
}