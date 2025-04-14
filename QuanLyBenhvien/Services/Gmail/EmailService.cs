using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace Fontend.Services.Gmail
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationCodeAsync(string email, string code)
        {
            try
            {
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var appPassword = _configuration["EmailSettings:AppPassword"];
                Console.WriteLine($"Sending email to {email} with code {code}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EHospital", senderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Mã xác thực đăng ký EHospital";

                message.Body = new TextPart("plain")
                {
                    Text = $"Mã xác thực của bạn là: {code}\nMã có hiệu lực trong 30 phút."
                };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                Console.WriteLine("Connected to SMTP server");
                await client.AuthenticateAsync(senderEmail, appPassword);
                Console.WriteLine("Authenticated with SMTP server");
                await client.SendAsync(message);
                Console.WriteLine("Email sent successfully");
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
    }
}
