using System.Net;
using System.Net.Mail;

namespace HappyLunchBE.Services
{
    public static class EmailService
    {
        public static void SendConfirmationEmail(string toEmail, string code)
        {
            var fromEmail = "thainguyenvuquang1@gmail.com";
            var fromPassword = "mlpqjqlvxxqxvwjr"; 
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Mã xác nhận đăng ký tài khoản",
                Body = $"Mã xác nhận đăng ký tài khoản là: {code}"
            };

            smtp.Send(message);
        }
    }
}
