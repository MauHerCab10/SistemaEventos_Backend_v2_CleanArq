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
    
    //Autentica a un usuario utilizando sus credenciales (email y contraseña)
    public async Task<Respuesta<UsuarioResponseDTO>> AutenticarUsuario(UsuarioLoginRequestDTO DTOUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmail(DTOUsuario.Email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("No se encontraron coincidencias con esas credenciales. Favor revisar los datos con los que esta intentando acceder al sistema.");
            }

            var contrasenaValidada = _passwordHasher.VerificarContrasena(DTOUsuario.Contrasena, usuarioEncontrado.ContrasenaHash);
            if (!usuarioEncontrado.Confirmado && !usuarioEncontrado.Restablecer && !string.IsNullOrEmpty(usuarioEncontrado.ContrasenaHash))
            {
                return Respuesta<UsuarioResponseDTO>.Fail($"Falta por confirmar su cuenta. Se envio un correo de solicitud de confirmacion a '{DTOUsuario.Email}'.");
            }

            if (usuarioEncontrado.Restablecer && !usuarioEncontrado.Confirmado && string.IsNullOrEmpty(usuarioEncontrado.ContrasenaHash))
            {
                return Respuesta<UsuarioResponseDTO>.Fail($"Se ha solicitado restablecer su cuenta. Favor revise la bandeja de su correo '{DTOUsuario.Email}'.");
            }

            if (!contrasenaValidada)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("La contrasena no coincide con la que hay almacenada en el sistema.");
            }

            var resultadoTokens = await _autorizacionService.GenerarTokensConCredenciales(DTOUsuario.Email, cancellationToken);
            if (!resultadoTokens.IsSuccess || resultadoTokens.Valor is null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail(resultadoTokens.Mensaje);
            }

            return Respuesta<UsuarioResponseDTO>.Ok(
                MapToResponse(usuarioEncontrado, resultadoTokens.Valor),
                "Autenticacion exitosa.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Registra un nuevo usuario en el sistema utilizando los datos proporcionados
    public async Task<Respuesta<UsuarioResponseDTO>> RegistrarUsuario(UsuarioRegistroRequestDTO DTOUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var existeUsuario = await _usuarioRepository.ConsultarUsuarioPorEmail(DTOUsuario.Email, cancellationToken);
            if (existeUsuario is not null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("El correo electronico proporcionado ya se encuentra registrado en el sistema. Por favor acceda con sus credenciales de acceso.");
            }

            var validacion = ValidarRegistro(DTOUsuario.NombreApellido, DTOUsuario.Email, DTOUsuario.Contrasena);
            if (!validacion.IsSuccess)
            {
                return validacion;
            }

            var fechaActual = _dateTimeProvider.ObtenerDateTimeActual();
            var usuario = new Usuario
            {
                NombreApellido = DTOUsuario.NombreApellido,
                Email = DTOUsuario.Email
            };

            usuario.PrepararNuevoRegistro(
                _passwordHasher.EncriptarContraseña(DTOUsuario.Contrasena),
                _passwordHasher.GenerarGuid(),
                fechaActual,
                _guidAccessSettings.GuidAccesoExpirationMinutes);

            var respuesta = await _usuarioRepository.RegistrarUsuario(usuario, cancellationToken);
            if (!respuesta)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("No se pudo crear su cuenta.");
            }

            return await EnviarCorreoConPlantilla(
                PlantillasCorreoEnum.ConfirmarCorreo,
                usuario.NombreApellido,
                usuario.Email,
                _accountUrlBuilder.ArmarUrlConfirmacionCuenta(usuario.GuidAcceso),
                $"Su cuenta ha sido creada satisfactoriamente. Hemos enviado un mensaje al correo '{usuario.Email}' para confirmar su cuenta.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Permite a un usuario iniciar el proceso de restablecimiento de contraseña proporcionando su correo electrónico
    public async Task<Respuesta<UsuarioResponseDTO>> OlvidoSuContrasena(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmail(email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("No se encontraron coincidencias con el correo proporcionado.");
            }

            var fechaActual = _dateTimeProvider.ObtenerDateTimeActual();
            usuarioEncontrado.PrepararRestablecimiento(
                _passwordHasher.GenerarGuid(),
                fechaActual,
                _guidAccessSettings.GuidAccesoExpirationMinutes);

            var respuesta = await _usuarioRepository.RestablecerContrasena(usuarioEncontrado, cancellationToken);
            if (!respuesta)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("No se pudo restablecer su contrasena.");
            }

            return await EnviarCorreoConPlantilla(
                PlantillasCorreoEnum.RestablecerContrasena,
                usuarioEncontrado.NombreApellido,
                usuarioEncontrado.Email,
                _accountUrlBuilder.ArmarUrlRestablecerContrasena(usuarioEncontrado.GuidAcceso),
                "La solicitud de restablecimiento de contrasena fue procesada satisfactoriamente. Por favor revise la bandeja de entrada de su correo electronico para actualizar su contrasena.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Permite a un usuario actualizar su contraseña utilizando un enlace de restablecimiento válido
    public async Task<Respuesta<UsuarioResponseDTO>> ActualizarContrasenaAntigua(string guidAcceso, string nuevaContrasena, string confirmacionContrasena, CancellationToken cancellationToken = default)
    {
        try
        {
            if (nuevaContrasena != confirmacionContrasena)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("Las contrasenas ingresadas no coinciden.");
            }

            var passwordValidation = ValidarFormatoContrasena(nuevaContrasena);
            if (!passwordValidation.IsSuccess)
            {
                return passwordValidation;
            }

            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorGuid(guidAcceso, cancellationToken);
            if (usuarioEncontrado is null || !usuarioEncontrado.GuidActivo)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("Solicitud no existe o ya se encuentra invalida.");
            }

            if (usuarioEncontrado.GuidValidado || !usuarioEncontrado.GuidActivo)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("El enlace por el cual solicitaste el cambio de contrasena ya se encuentra invalido, ha expirado, o ya habias realizado un cambio de contrasena anteriormente usando este correo.");
            }

            var contrasenaHash = _passwordHasher.EncriptarContraseña(nuevaContrasena);
            var respuesta = await _usuarioRepository.ActualizarContrasenaAntigua(guidAcceso, contrasenaHash, cancellationToken);

            return respuesta
                ? Respuesta<UsuarioResponseDTO>.Ok(null, "Contrasena actualizada satisfactoriamente.")
                : Respuesta<UsuarioResponseDTO>.Fail("No se pudo actualizar la contrasena. Favor usar el correo con la ultima solicitud de cambio de contrasena generada.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Permite a un usuario confirmar su cuenta utilizando un enlace de confirmación válido
    public async Task<Respuesta<UsuarioResponseDTO>> ConfirmarCuenta(string guidAcceso, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorGuid(guidAcceso, cancellationToken);
            if (usuarioEncontrado is null || !usuarioEncontrado.GuidActivo)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("GUID no existe o ya se encuentra invalido. Favor solicite el restablecimiento de su contrasena.");
            }

            if (usuarioEncontrado.Confirmado)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("La cuenta ya fue confirmada anteriormente.");
            }

            var respuesta = await _usuarioRepository.ConfirmarCuenta(guidAcceso, cancellationToken);
            return respuesta
                ? Respuesta<UsuarioResponseDTO>.Ok(null, "Confirmacion de cuenta realizada satisfactoriamente.")
                : Respuesta<UsuarioResponseDTO>.Fail("No se pudo confirmar la cuenta.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Permite a un usuario autenticarse utilizando su cuenta de Google
    public async Task<Respuesta<UsuarioResponseDTO>> AutenticarUsuarioGoogle(UsuarioGoogleRequestDTO DTOUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioEncontrado = await _usuarioRepository.ConsultarUsuarioPorEmail(DTOUsuario.Email, cancellationToken);
            if (usuarioEncontrado is null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("No se encontraron coincidencias con esas credenciales. Favor revisar los datos con los que esta intentando acceder al sistema.");
            }

            if (usuarioEncontrado.Restablecer && !usuarioEncontrado.Confirmado && string.IsNullOrEmpty(usuarioEncontrado.ContrasenaHash))
            {
                return Respuesta<UsuarioResponseDTO>.Fail($"Se ha solicitado restablecer su cuenta. Favor revise la bandeja de su correo '{DTOUsuario.Email}'.");
            }

            var resultadoTokens = await _autorizacionService.GenerarTokensConCredenciales(DTOUsuario.Email, cancellationToken);
            if (!resultadoTokens.IsSuccess || resultadoTokens.Valor is null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail(resultadoTokens.Mensaje);
            }

            return Respuesta<UsuarioResponseDTO>.Ok(
                MapToResponse(usuarioEncontrado, resultadoTokens.Valor),
                "Autenticacion exitosa.");
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Permite a un usuario registrar una nueva cuenta asociada a esa dirección de correo electrónico utilizando su cuenta de Google
    public async Task<Respuesta<UsuarioResponseDTO>> RegistrarUsuarioGoogle(UsuarioGoogleRequestDTO DTOUsuario, CancellationToken cancellationToken = default)
    {
        try
        {
            var existeUsuario = await _usuarioRepository.ConsultarUsuarioPorEmail(DTOUsuario.Email, cancellationToken);
            if (existeUsuario is not null)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("El correo electronico proporcionado ya se encuentra registrado en el sistema. Por favor acceda con otra cuenta.");
            }

            var fechaActual = _dateTimeProvider.ObtenerDateTimeActual();
            var usuario = new Usuario
            {
                NombreApellido = DTOUsuario.Nombre,
                Email = DTOUsuario.Email
            };

            usuario.PrepararNuevoRegistro(
                _passwordHasher.EncriptarContraseña(DTOUsuario.GoogleSub),
                _passwordHasher.GenerarGuid(),
                fechaActual,
                _guidAccessSettings.GuidAccesoExpirationMinutes);

            var respuesta = await _usuarioRepository.RegistrarUsuario(usuario, cancellationToken);
            if (!respuesta)
            {
                return Respuesta<UsuarioResponseDTO>.Fail("No se pudo crear su cuenta.");
            }

            return await EnviarCorreoConPlantilla(
                PlantillasCorreoEnum.ConfirmarCorreo,
                usuario.NombreApellido,
                usuario.Email,
                _accountUrlBuilder.ArmarUrlConfirmacionCuenta(usuario.GuidAcceso),
                $"Su cuenta ha sido creada satisfactoriamente. Hemos enviado un mensaje al correo '{usuario.Email}' para confirmar su cuenta.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            return Respuesta<UsuarioResponseDTO>.Fail(exception.Message);
        }
    }

    //Mapea un objeto Usuario y AuthTokensDTO a un UsuarioResponseDTO
    private static UsuarioResponseDTO MapToResponse(Usuario usuario, AuthTokensDTO tokens)
    {
        return new UsuarioResponseDTO
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = ObtenerPrimerNombre(usuario.NombreApellido),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        };
    }

    //Obtiene el primer nombre de un nombre completo
    private static string ObtenerPrimerNombre(string nombreCompleto)
    {
        return nombreCompleto
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? nombreCompleto;
    }

    //Valida los datos ingresados por el usuario durante el proceso de registro, asegurando que se cumplan los requisitos de formato para el nombre, correo electrónico y contraseña
    private static Respuesta<UsuarioResponseDTO> ValidarRegistro(string nombreApellido, string email, string contrasena)
    {
        if (string.IsNullOrEmpty(nombreApellido))
        {
            return Respuesta<UsuarioResponseDTO>.Fail("Campo de Nombre y Apellido es obligatorio.");
        }

        if (!EmailRegex.IsMatch(email))
        {
            return Respuesta<UsuarioResponseDTO>.Fail("Formato de correo electronico invalido.");
        }

        return ValidarFormatoContrasena(contrasena);
    }

    //Valida que la contraseña cumpla con los requisitos de formato establecidos
    private static Respuesta<UsuarioResponseDTO> ValidarFormatoContrasena(string contrasena)
    {
        if (contrasena.Length < 12 || !PasswordRegex.IsMatch(contrasena))
        {
            return Respuesta<UsuarioResponseDTO>.Fail("Formato de contrasena invalido. La contrasena debe contener 12 caracteres como minimo, al menos una minuscula, una mayuscula, un numero, un caracter especial y no debe contener espacios.");
        }

        return Respuesta<UsuarioResponseDTO>.Ok(null);
    }

    //Envía un correo electrónico al usuario utilizando una plantilla específica, personalizando el contenido con su nombre y un enlace relevante para la acción que se está realizando ("confirmación de cuenta" o "restablecimiento de contraseña")
    private async Task<Respuesta<UsuarioResponseDTO>> EnviarCorreoConPlantilla(
        PlantillasCorreoEnum tipoPlantilla,
        string nombreUsuario,
        string email,
        string url,
        string mensajeExito,
        CancellationToken cancellationToken)
    {
        var plantillaCorreo = await _plantillaCorreoProvider.ObtenerPlantillaPorTipo(tipoPlantilla, cancellationToken);
        if (plantillaCorreo is null)
        {
            return Respuesta<UsuarioResponseDTO>.Fail("No fue posible cargar la plantilla del correo.");
        }

        var htmlBody = string.Format(plantillaCorreo.Cuerpo, nombreUsuario, url);
        var infoCorreo = new InfoCorreo
        {
            Asunto = plantillaCorreo.Asunto,
            Para = email,
            Contenido = htmlBody
        };

        var correoEnviado = await _emailSender.EnviarCorreo(infoCorreo, cancellationToken);
        return correoEnviado
            ? Respuesta<UsuarioResponseDTO>.Ok(null, mensajeExito)
            : Respuesta<UsuarioResponseDTO>.Fail($"No fue posible enviar el correo a '{email}'.");
    }
}