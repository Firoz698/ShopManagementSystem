namespace ShopManagementSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtp = _config.GetSection("SmtpSettings");
            using var client = new System.Net.Mail.SmtpClient(smtp["Host"], int.Parse(smtp["Port"]!))
            {
                Credentials = new System.Net.NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = true
            };

            var mail = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(smtp["Username"]!, "Shop Management System"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}