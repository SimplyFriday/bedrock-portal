using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MinecraftWrapper.Services
{
    public class SendGridSender : IEmailSender
    {
        private readonly ApplicationSettings _applicationSettings;

        public SendGridSender ( IOptions<ApplicationSettings> options )
        {
            _applicationSettings = options.Value;
        }

        public Task SendEmailAsync ( string email, string subject, string htmlMessage )
        {
            var msg = new SendGridMessage ();

            msg.SetFrom ( new EmailAddress ( _applicationSettings.SystemFromEmailAddress ) );
            msg.AddTos ( new List<EmailAddress> { new EmailAddress ( email ) } );
            msg.SetSubject ( subject );
            msg.AddContent ( MimeType.Html, htmlMessage );
            
            var client = new SendGridClient ( _applicationSettings.SendGridApiKey );
            var response = client.SendEmailAsync ( msg );

            return response;
        }
    }
}
