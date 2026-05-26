using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Server.Services;

namespace SistemaEventos.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IAutorizacionService _autorizacionService;
    private readonly ICookieService _cookieService;
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IAutorizacionService autorizacionService,
        ICookieService cookieService,
        IUsuarioService usuarioService)
    {
        _configuration = configuration;
        _environment = environment;
        _autorizacionService = autorizacionService;
        _cookieService = cookieService;
        _usuarioService = usuarioService;
    }

    [AllowAnonymous]
    [HttpPost("RegistrarUsuario")]
    public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroRequestDto usuario)
    {
        var resultado = await _usuarioService.RegistrarUsuarioAsync(usuario, HttpContext.RequestAborted);
        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [AllowAnonymous]
    [HttpGet("ConfirmarCuenta")]
    public async Task<IActionResult> ConfirmarCuenta(string guidAcceso)
    {
        var resultado = await _usuarioService.ConfirmarCuentaAsync(guidAcceso, HttpContext.RequestAborted);
        var frontendBaseUrl = _environment.IsDevelopment()
            ? _configuration["Frontend_URLs:Desarrollo"]
            : _configuration["Frontend_URLs:Produccion"];

        return Redirect($"{frontendBaseUrl}/login?confirmacion={(resultado.IsSuccess ? "ok" : "error")}");
    }

    [AllowAnonymous]
    [HttpPost("AutenticarUsuario")]
    public async Task<IActionResult> AutenticarUsuario([FromBody] UsuarioLoginRequestDto usuario)
    {
        var resultado = await _usuarioService.AutenticarUsuarioAsync(usuario, HttpContext.RequestAborted);
        if (!resultado.IsSuccess || resultado.Valor is null)
        {
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        _cookieService.SetCookieAccessToken(resultado.Valor.AccessToken);
        _cookieService.SetCookieRefreshToken(resultado.Valor.RefreshToken);

        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje,
            idUsuario = resultado.Valor.IdUsuario,
            nombreUsuario = resultado.Valor.NombreUsuario,
            accessToken = resultado.Valor.AccessToken
        });
    }

    [AllowAnonymous]
    [HttpPost("AutenticarUsuarioGoogle")]
    public async Task<IActionResult> AutenticarUsuarioGoogle([FromBody] UsuarioGoogleRequestDto usuario)
    {
        var resultado = await _usuarioService.AutenticarUsuarioGoogleAsync(usuario, HttpContext.RequestAborted);
        if (!resultado.IsSuccess || resultado.Valor is null)
        {
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        _cookieService.SetCookieAccessToken(resultado.Valor.AccessToken);
        _cookieService.SetCookieRefreshToken(resultado.Valor.RefreshToken);

        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje,
            idUsuario = resultado.Valor.IdUsuario,
            nombreUsuario = resultado.Valor.NombreUsuario,
            accessToken = resultado.Valor.AccessToken
        });
    }

    [AllowAnonymous]
    [HttpPost("RegistrarUsuarioGoogle")]
    public async Task<IActionResult> RegistrarUsuarioGoogle([FromBody] UsuarioGoogleRequestDto usuario)
    {
        var resultado = await _usuarioService.RegistrarUsuarioGoogleAsync(usuario, HttpContext.RequestAborted);
        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [AllowAnonymous]
    [HttpPost("OlvidoSuContrasena")]
    public async Task<IActionResult> OlvidoSuContrasena([FromBody] EmailDto email)
    {
        var resultado = await _usuarioService.OlvidoSuContrasenaAsync(email.Email, HttpContext.RequestAborted);
        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [AllowAnonymous]
    [HttpPost("RestablecerContrasena")]
    public async Task<IActionResult> RestablecerContrasena([FromBody] ActualizarContrasenaDto contrasena)
    {
        var resultado = await _usuarioService.ActualizarContrasenaAntiguaAsync(
            contrasena.GuidAcceso,
            contrasena.NuevaContrasena,
            contrasena.ConfirmacionContrasena,
            HttpContext.RequestAborted);

        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [Authorize]
    [HttpPost("CerrarSesion")]
    public async Task<IActionResult> CerrarSesion()
    {
        var idUsuario = HttpContext.Items["IdUsuario"]?.ToString();
        if (!int.TryParse(idUsuario, out var userId))
        {
            return BadRequest(new { isSuccess = false, mensaje = "No fue posible determinar el usuario autenticado." });
        }

        var response = await _autorizacionService.CerrarSesionAsync(userId, HttpContext.RequestAborted);
        if (!response.IsSuccess)
        {
            return BadRequest(response);
        }

        _cookieService.EliminarCookiesDelUsuario();
        return Ok(response);
    }

    [Authorize]
    [HttpGet("Ping")]
    public IActionResult Ping()
    {
        var idUsuario = HttpContext.Items["IdUsuario"]?.ToString();
        var accessToken = HttpContext.Items["AccessToken"]?.ToString();
        var refreshToken = HttpContext.Items["RefreshToken"]?.ToString();

        return Ok(new
        {
            message = "Pong",
            timestamp = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss tt"),
            idUser = idUsuario,
            refreshToken,
            accessToken
        });
    }
}