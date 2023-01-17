using System.Net.Mail;
using System;

namespace QuizMe.Utils
{
    public class EmailSender
    {
        public bool Send(string to, string subject, string messageBody)
        {
            var message = new MailMessage("ahaan.news98@gmail.com", to)
            {
                Subject = subject,
                IsBodyHtml = true,
                Body = messageBody,
            };
            var client = new SmtpClient() { EnableSsl = true };
            client.Credentials = new System.Net.NetworkCredential("ahaan.news98@gmail.com", "GQyBFO4nKPVMUAqJ");
            client.Host = "smtp-relay.sendinblue.com";
            client.Port = 587;
            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
