
using MailKit.Net.Smtp;
using MimeKit;

namespace IdentityService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration,ILogger<EmailService> logger )
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message= new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["Email:FromName"], _configuration["Email:FromAddress"]));
            message.To.Add(new MailboxAddress(to,to));
            message.Subject = subject;

            var builder= new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            using var client=new SmtpClient();

            try
            {

                _logger.LogInformation("Connecting to {Host}:{Port} as {User}",
    _configuration["Email:SmtpHost"],
    _configuration["Email:SmtpPort"],
    _configuration["Email:Username"]);

                await client.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
    }
}
