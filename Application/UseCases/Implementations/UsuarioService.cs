using System.Text.RegularExpressions;
using SistemaEventos.Application.Common.Models;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.Interfaces.Persistence;
using SistemaEventos.Application.Interfaces.Services;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Domain.Entities;
using SistemaEventos.Domain.Enums;

namespace SistemaEventos.Application.UseCases.Implementations;

public class UsuarioService : IUsuarioService
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[^\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IAccountUrlBuilder _accountUrlBuilder;
    private readonly IAutorizacionService _autorizacionService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailSender _emailSender;
    private readonly IGuidAccessSettings _guidAccessSettings;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPlantillaCorreoProvider _plantillaCorreoProvider;
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(
        IAccountUrlBuilder accountUrlBuilder,
        IAutorizacionService autorizacionService,
        IDateTimeProvider dateTimeProvider,
        IEmailSender emailSender,
        IGuidAccessSettings guidAccessSettings,
        IPasswordHasher passwordHasher,
        IPlantillaCorreoProvider plantillaCorreoProvider,
        IUsuarioRepository usuarioRepository)
    {
        _accountUrlBuilder = accountUrlBuilder;
        _autorizacionService = autorizacionService;
        _dateTimeProvider = dateTimeProvider;
        _emailSender = emailSender;
        _guidAccessSettings = guidAccessSettings;
        _passwordHasher = passwordHasher;
        _plantillaCorreoProvider = plantillaCorreoProvider;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Respuesta<UsuarioResponseDto>> AutenticarUsuarioAsync(UsuarioLoginRequestDto dtoUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmailAsync(dtoUsuario.Email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<UsuarioResponseDto>.Fail("No se encontraron coincidencias con esas credenciales. Favor revisar los datos con los que esta intentando acceder al sistema.");
            }

            var contrasenaValidada = _passwordHasher.Verify(dtoUsuario.Contrasena, usuarioEncontrado.ContrasenaHash);
            if (!usuarioEncontrado.Confirmado && !usuarioEncontrado.Restablecer && !string.IsNullOrEmpty(usuarioEncontrado.ContrasenaHash))
            {
                return Respuesta<UsuarioResponseDto>.Fail($"Falta por confirmar su cuenta. Se envio un correo de solicitud de confirmacion a '{dtoUsuario.Email}'.");
            }

            if (usuarioEncontrado.Restablecer && !usuarioEncontrado.Confirmado && string.IsNullOrEmpty(usuarioEncontrado.ContrasenaHash))
            {
                return Respuesta<UsuarioResponseDto>.Fail($"Se ha solicitado restablecer su cuenta. Favor revise la bandeja de su correo '{dtoUsuario.Email}'.");
            }

            if (!contrasenaValidada)
            {
                return Respuesta<UsuarioResponseDto>.Fail("La contrasena no coincide con la que hay almacenada en el sistema.");
            }

            var resultadoTokens = await _autorizacionService.GenerarTokensConCredencialesAsync(dtoUsuario.Email, cancellationToken);
            if (!resultadoTokens.IsSuccess || resultadoTokens.Valor is null)
            {
                return Respuesta<UsuarioResponseDto>.Fail(resultadoTokens.Mensaje);
            }

            return Respuesta<UsuarioResponseDto>.Ok(
                MapToResponse(usuarioEncontrado, resultadoTokens.Valor),
                "Autenticacion exitosa.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<UsuarioResponseDto>> RegistrarUsuarioAsync(UsuarioRegistroRequestDto dtoUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var existeUsuario = await _usuarioRepository.ConsultarUsuarioPorEmailAsync(dtoUsuario.Email, cancellationToken);
            if (existeUsuario is not null)
            {
                return Respuesta<UsuarioResponseDto>.Fail("El correo electronico proporcionado ya se encuentra registrado en el sistema. Por favor acceda con sus credenciales de acceso.");
            }

            var validacion = ValidarRegistro(dtoUsuario.NombreApellido, dtoUsuario.Email, dtoUsuario.Contrasena);
            if (!validacion.IsSuccess)
            {
                return validacion;
            }

            var fechaActual = _dateTimeProvider.GetCurrentDateTime();
            var usuario = new Usuario
            {
                NombreApellido = dtoUsuario.NombreApellido,
                Email = dtoUsuario.Email
            };

            usuario.PrepararNuevoRegistro(
                _passwordHasher.Hash(dtoUsuario.Contrasena),
                _passwordHasher.GenerarGuid(),
                fechaActual,
                _guidAccessSettings.GuidAccesoExpirationMinutes);

            var respuesta = await _usuarioRepository.RegistrarUsuarioAsync(usuario, cancellationToken);
            if (!respuesta)
            {
                return Respuesta<UsuarioResponseDto>.Fail("No se pudo crear su cuenta.");
            }

            return await EnviarCorreoConPlantillaAsync(
                PlantillasCorreoEnum.ConfirmarCorreo,
                usuario.NombreApellido,
                usuario.Email,
                _accountUrlBuilder.BuildConfirmacionCuentaUrl(usuario.GuidAcceso),
                $"Su cuenta ha sido creada satisfactoriamente. Hemos enviado un mensaje al correo '{usuario.Email}' para confirmar su cuenta.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<UsuarioResponseDto>> OlvidoSuContrasenaAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmailAsync(email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<UsuarioResponseDto>.Fail("No se encontraron coincidencias con el correo proporcionado.");
            }

            var fechaActual = _dateTimeProvider.GetCurrentDateTime();
            usuarioEncontrado.PrepararRestablecimiento(
                _passwordHasher.GenerarGuid(),
                fechaActual,
                _guidAccessSettings.GuidAccesoExpirationMinutes);

            var respuesta = await _usuarioRepository.RestablecerContrasenaAsync(usuarioEncontrado, cancellationToken);
            if (!respuesta)
            {
                return Respuesta<UsuarioResponseDto>.Fail("No se pudo restablecer su contrasena.");
            }

            return await EnviarCorreoConPlantillaAsync(
                PlantillasCorreoEnum.RestablecerContrasena,
                usuarioEncontrado.NombreApellido,
                usuarioEncontrado.Email,
                _accountUrlBuilder.BuildRestablecerContrasenaUrl(usuarioEncontrado.GuidAcceso),
                "La solicitud de restablecimiento de contrasena fue procesada satisfactoriamente. Por favor revise la bandeja de entrada de su correo electronico para actualizar su contrasena.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<UsuarioResponseDto>> ActualizarContrasenaAntiguaAsync(string guidAcceso, string nuevaContrasena, string confirmacionContrasena, CancellationToken cancellationToken = default)
    {
        try
        {
            if (nuevaContrasena != confirmacionContrasena)
            {
                return Respuesta<UsuarioResponseDto>.Fail("Las contrasenas ingresadas no coinciden.");
            }

            var passwordValidation = ValidarFormatoContrasena(nuevaContrasena);
            if (!passwordValidation.IsSuccess)
            {
                return passwordValidation;
            }

            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorGuidAsync(guidAcceso, cancellationToken);
            if (usuarioEncontrado is null || !usuarioEncontrado.GuidActivo)
            {
                return Respuesta<UsuarioResponseDto>.Fail("Solicitud no existe o ya se encuentra invalida.");
            }

            if (usuarioEncontrado.GuidValidado || !usuarioEncontrado.GuidActivo)
            {
                return Respuesta<UsuarioResponseDto>.Fail("El enlace por el cual solicitaste el cambio de contrasena ya se encuentra invalido, ha expirado, o ya habias realizado un cambio de contrasena anteriormente usando este correo.");
            }

            var contrasenaHash = _passwordHasher.Hash(nuevaContrasena);
            var respuesta = await _usuarioRepository.ActualizarContrasenaAntiguaAsync(guidAcceso, contrasenaHash, cancellationToken);

            return respuesta
                ? Respuesta<UsuarioResponseDto>.Ok(null, "Contrasena actualizada satisfactoriamente.")
                : Respuesta<UsuarioResponseDto>.Fail("No se pudo actualizar la contrasena. Favor usar el correo con la ultima solicitud de cambio de contrasena generada.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<UsuarioResponseDto>> ConfirmarCuentaAsync(string guidAcceso, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorGuidAsync(guidAcceso, cancellationToken);
            if (usuarioEncontrado is null || !usuarioEncontrado.GuidActivo)
            {
                return Respuesta<UsuarioResponseDto>.Fail("GUID no existe o ya se encuentra invalido. Favor solicite el restablecimiento de su contrasena.");
            }

            if (usuarioEncontrado.Confirmado)
            {
                return Respuesta<UsuarioResponseDto>.Fail("La cuenta ya fue confirmada anteriormente.");
            }

            var respuesta = await _usuarioRepository.ConfirmarCuentaAsync(guidAcceso, cancellationToken);
            return respuesta
                ? Respuesta<UsuarioResponseDto>.Ok(null, "Confirmacion de cuenta realizada satisfactoriamente.")
                : Respuesta<UsuarioResponseDto>.Fail("No se pudo confirmar la cuenta.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<UsuarioResponseDto>> AutenticarUsuarioGoogleAsync(UsuarioGoogleRequestDto dtoUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmailAsync(dtoUsuario.Email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<UsuarioResponseDto>.Fail("No se encontraron coincidencias con esas credenciales. Favor revisar los datos con los que esta intentando acceder al sistema.");
            }

            if (usuarioEncontrado.Restablecer && !usuarioEncontrado.Confirmado && string.IsNullOrEmpty(usuarioEncontrado.ContrasenaHash))
            {
                return Respuesta<UsuarioResponseDto>.Fail($"Se ha solicitado restablecer su cuenta. Favor revise la bandeja de su correo '{dtoUsuario.Email}'.");
            }

            var resultadoTokens = await _autorizacionService.GenerarTokensConCredencialesAsync(dtoUsuario.Email, cancellationToken);
            if (!resultadoTokens.IsSuccess || resultadoTokens.Valor is null)
            {
                return Respuesta<UsuarioResponseDto>.Fail(resultadoTokens.Mensaje);
            }

            return Respuesta<UsuarioResponseDto>.Ok(
                MapToResponse(usuarioEncontrado, resultadoTokens.Valor),
                "Autenticacion exitosa.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    public async Task<Respuesta<UsuarioResponseDto>> RegistrarUsuarioGoogleAsync(UsuarioGoogleRequestDto dtoUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var existeUsuario = await _usuarioRepository.ConsultarUsuarioPorEmailAsync(dtoUsuario.Email, cancellationToken);
            if (existeUsuario is not null)
            {
                return Respuesta<UsuarioResponseDto>.Fail("El correo electronico proporcionado ya se encuentra registrado en el sistema. Por favor acceda con otra cuenta.");
            }

            var fechaActual = _dateTimeProvider.GetCurrentDateTime();
            var usuario = new Usuario
            {
                NombreApellido = dtoUsuario.Nombre,
                Email = dtoUsuario.Email
            };

            usuario.PrepararNuevoRegistro(
                _passwordHasher.Hash(dtoUsuario.GoogleSub),
                _passwordHasher.GenerarGuid(),
                fechaActual,
                _guidAccessSettings.GuidAccesoExpirationMinutes);

            var respuesta = await _usuarioRepository.RegistrarUsuarioAsync(usuario, cancellationToken);
            if (!respuesta)
            {
                return Respuesta<UsuarioResponseDto>.Fail("No se pudo crear su cuenta.");
            }

            return await EnviarCorreoConPlantillaAsync(
                PlantillasCorreoEnum.ConfirmarCorreo,
                usuario.NombreApellido,
                usuario.Email,
                _accountUrlBuilder.BuildConfirmacionCuentaUrl(usuario.GuidAcceso),
                $"Su cuenta ha sido creada satisfactoriamente. Hemos enviado un mensaje al correo '{usuario.Email}' para confirmar su cuenta.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDto>.Fail(exception.Message);
        }
    }

    private static UsuarioResponseDto MapToResponse(Usuario usuario, AuthTokensDto tokens)
    {
        return new UsuarioResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = ObtenerPrimerNombre(usuario.NombreApellido),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        };
    }

    private static string ObtenerPrimerNombre(string nombreCompleto)
    {
        return nombreCompleto
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? nombreCompleto;
    }

    private static Respuesta<UsuarioResponseDto> ValidarRegistro(string nombreApellido, string email, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreApellido))
        {
            return Respuesta<UsuarioResponseDto>.Fail("Campo de Nombre y Apellido es obligatorio.");
        }

        if (!EmailRegex.IsMatch(email))
        {
            return Respuesta<UsuarioResponseDto>.Fail("Formato de correo electronico invalido.");
        }

        return ValidarFormatoContrasena(contrasena);
    }

    private static Respuesta<UsuarioResponseDto> ValidarFormatoContrasena(string contrasena)
    {
        if (contrasena.Length < 12 || !PasswordRegex.IsMatch(contrasena))
        {
            return Respuesta<UsuarioResponseDto>.Fail("Formato de contrasena invalido. La contrasena debe contener 12 caracteres como minimo, al menos una minuscula, una mayuscula, un numero, un caracter especial y no debe contener espacios.");
        }

        return Respuesta<UsuarioResponseDto>.Ok(null);
    }

    private async Task<Respuesta<UsuarioResponseDto>> EnviarCorreoConPlantillaAsync(
        PlantillasCorreoEnum tipoPlantilla,
        string nombreUsuario,
        string email,
        string url,
        string mensajeExito,
        CancellationToken cancellationToken)
    {
        var plantillaCorreo = await _plantillaCorreoProvider.ObtenerPorTipoAsync(tipoPlantilla, cancellationToken);
        if (plantillaCorreo is null)
        {
            return Respuesta<UsuarioResponseDto>.Fail("No fue posible cargar la plantilla del correo.");
        }

        var htmlBody = string.Format(plantillaCorreo.Cuerpo, nombreUsuario, url);
        var infoCorreo = new InfoCorreo
        {
            Asunto = plantillaCorreo.Asunto,
            Para = email,
            Contenido = htmlBody
        };

        var correoEnviado = await _emailSender.EnviarAsync(infoCorreo, cancellationToken);
        return correoEnviado
            ? Respuesta<UsuarioResponseDto>.Ok(null, mensajeExito)
            : Respuesta<UsuarioResponseDto>.Fail($"No fue posible enviar el correo a '{email}'.");
    }
}