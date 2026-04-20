using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace HranitelPROGeneralDepartmentTerminal.Services
{
    public static class EmailService
    {
        // Настройте под свой SMTP-сервер
        private static readonly string SmtpHost = "smtp.yandex.ru";
        private static readonly int SmtpPort = 587;
        private static readonly string SmtpUsername = "your-email@yandex.ru";
        private static readonly string SmtpPassword = "your-password";
        private static readonly bool EnableSsl = true;

        public static void SendEmails(List<string> toEmails, string subject, string body)
        {
            if (toEmails == null || toEmails.Count == 0) return;

            try
            {
                using (var client = new SmtpClient(SmtpHost, SmtpPort))
                {
                    client.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                    client.EnableSsl = EnableSsl;

                    foreach (var email in toEmails)
                    {
                        var mail = new MailMessage(SmtpUsername, email)
                        {
                            Subject = subject,
                            Body = body
                        };
                        client.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // В реальном приложении – логирование
                Console.WriteLine($"Ошибка отправки email: {ex.Message}");
                // Можно показать MessageBox, но в сервисе лучше не использовать UI-зависимости
            }
        }
    }
}