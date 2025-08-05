using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using api.Configuration;
using api.Interfaces;
using Microsoft.Extensions.Options;

namespace api.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtp;

        public EmailSender(IOptions<SmtpSettings> options)
        {
            _smtp = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var msg = new MailMessage(_smtp.From, email, subject, htmlMessage)
            { IsBodyHtml = true };
            using var client = new SmtpClient(_smtp.Host, _smtp.Port);
            await client.SendMailAsync(msg);
        }


    }
}