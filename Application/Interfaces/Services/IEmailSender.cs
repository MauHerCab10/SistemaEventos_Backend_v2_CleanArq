using SistemaEventos.Application.Common.Models;

namespace SistemaEventos.Application.Interfaces.Services;

public interface IEmailSender
{
    Task<bool> EnviarCorreo(InfoCorreo request, CancellationToken cancellationToken = default);
}