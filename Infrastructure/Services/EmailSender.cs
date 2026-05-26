using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Infrastructure.Configuration;

namespace SistemaEventos.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly ServidorEmailOptions _emailOptions;

    public EmailSender(IOptions<ServidorEmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task<bool> EnviarAsync(InfoCorreo request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailOptions.Username));
            email.To.Add(MailboxAddress.Parse(request.Para));
            email.Subject = request.Asunto;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = request.Contenido
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailOptions.Host, _emailOptions.Port, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}