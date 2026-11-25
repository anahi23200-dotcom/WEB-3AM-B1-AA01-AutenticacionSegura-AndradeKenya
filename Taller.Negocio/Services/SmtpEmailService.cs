using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Taller.Negocio.Configuration;
using Taller.Negocio.Interfaces;

namespace Taller.Negocio.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host) ||
                string.IsNullOrWhiteSpace(_smtpSettings.RemitenteCorreo))
            {
                throw new InvalidOperationException(
                    "La configuración SMTP no está definida. Configura SmtpSettings en User Secrets.");
            }

            using var mensaje = new MailMessage
            {
                From = new MailAddress(_smtpSettings.RemitenteCorreo, _smtpSettings.RemitenteNombre),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var clienteSmtp = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_smtpSettings.Usuario) &&
                !string.IsNullOrWhiteSpace(_smtpSettings.Contrasena))
            {
                clienteSmtp.Credentials = new NetworkCredential(_smtpSettings.Usuario, _smtpSettings.Contrasena);
            }

            await clienteSmtp.SendMailAsync(mensaje);
        }
    }
}