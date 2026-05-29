using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaEventos.Application.DTOs;
using SistemaEventos.Application.UseCases.Interfaces;
using SistemaEventos.Server.Extensions;
using SistemaEventos.Server.Services;

namespace SistemaEventos.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly IAutorizacionService _autorizacionService;
    private readonly ICookieService _cookieService;
    private readonly IFrontendUrlBuilder _frontendUrlBuilder;
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(
        IAutorizacionService autorizacionService,
        ICookieService cookieService,
        IFrontendUrlBuilder frontendUrlBuilder,
        IUsuarioService usuarioService)
    {
        _autorizacionService = autorizacionService;
        _cookieService = cookieService;
        _frontendUrlBuilder = frontendUrlBuilder;
        _usuarioService = usuarioService;
    }

    [AllowAnonymous]
    [HttpPost("RegistrarUsuario")] //1ro (Sign Up)
    public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroRequestDTO usuario)
    {
        var resultado = await _usuarioService.RegistrarUsuario(usuario, HttpContext.RequestAborted);
        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [AllowAnonymous]
    [HttpGet("ConfirmarCuenta")] //2do (ejecutarlo mejor directamente desde el correo recibido)
    public async Task<IActionResult> ConfirmarCuenta(string guidAcceso)
    {
        var resultado = await _usuarioService.ConfirmarCuenta(guidAcceso, HttpContext.RequestAborted);
        return Redirect(_frontendUrlBuilder.ArmarUrlLoginConfirmacion(resultado.IsSuccess));
    }

    [AllowAnonymous]
    [HttpPost("AutenticarUsuario")] //3ro (Traditional Sign In)
    public async Task<IActionResult> AutenticarUsuario([FromBody] UsuarioLoginRequestDTO usuario)
    {
        var resultado = await _usuarioService.AutenticarUsuario(usuario, HttpContext.RequestAborted);
        if (!resultado.IsSuccess || resultado.Valor is null)
        {
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        //_cookieService.SetCookieAccessToken(resultado.Valor.AccessToken);
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
    [HttpPost("AutenticarUsuarioGoogle")] //4to (Google Sign In)
    public async Task<IActionResult> AutenticarUsuarioGoogle([FromBody] UsuarioGoogleRequestDTO usuario)
    {
        var resultado = await _usuarioService.AutenticarUsuarioGoogle(usuario, HttpContext.RequestAborted);
        if (!resultado.IsSuccess || resultado.Valor is null)
        {
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        //_cookieService.SetCookieAccessToken(resultado.Valor.AccessToken);
        _cookieService.SetCookieRefreshToken(resultado.Valor.RefreshToken);

        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje,
            idUsuario = resultado.Valor.IdUsuario,
            nombreUsuario = resultado.Valor.NombreUsuario,
            accessToken = resultado.Valor.AccessToken,
            /*refreshToken = resultado.Valor.RefreshToken*/
        });
    }

    [AllowAnonymous]
    [HttpPost("RegistrarUsuarioGoogle")] //5to (Google Sign Up)
    public async Task<IActionResult> RegistrarUsuarioGoogle([FromBody] UsuarioGoogleRequestDTO usuario)
    {
        var resultado = await _usuarioService.RegistrarUsuarioGoogle(usuario, HttpContext.RequestAborted);
        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [AllowAnonymous]
    [HttpPost("OlvidoSuContrasena")] //6to
    public async Task<IActionResult> OlvidoSuContrasena([FromBody] EmailDTO email)
    {
        var resultado = await _usuarioService.OlvidoSuContrasena(email.Email, HttpContext.RequestAborted);
        return Ok(new
        {
            isSuccess = resultado.IsSuccess,
            mensaje = resultado.Mensaje
        });
    }

    [AllowAnonymous]
    [HttpPost("RestablecerContrasena")] //7mo
    public async Task<IActionResult> RestablecerContrasena([FromBody] ActualizarContrasenaDTO contrasena)
    {
        var resultado = await _usuarioService.ActualizarContrasenaAntigua(
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

    //[Authorize]
    //[HttpPost("ObtenerRefreshToken")] //8vo (no lo uso en el Frontend, pero usarlo solo en caso de q se requiera generar un nuevo AccesToken y RefreshToken al mismo tiempo)
    //public async Task<IActionResult> ObtenerRefreshToken()
    //{
    //    var idUsuario = HttpContext.Items["IdUsuario"]?.ToString();
    //    var accessToken = HttpContext.Items["AccessToken"]?.ToString();
    //    var refreshToken = HttpContext.Items["RefreshToken"]?.ToString();

    //    var resultado = await _autorizacion.GenerarAccessTokenYRefreshTokenConRefreshTokenAnterior(int.Parse(idUsuario!), accessToken!, refreshToken!);

    //    if (resultado.IsSuccess)
    //    {
    //        // Cargar las cookies en el navegador del usuario
    //        //_cookies.SetCookieAccessToken(resultado.Valor.AccessToken);
    //        _cookies.SetCookieRefreshToken(resultado.Valor.RefreshToken);

    //        return Ok(new 
    //        { 
    //            isSuccess = resultado.IsSuccess, 
    //            mensaje = resultado.Mensaje, 
    //            accessToken = resultado.Valor.AccessToken 
    //        });
    //    }
    //    else
    //    {
    //        return BadRequest(resultado);
    //    }
    //}

    [Authorize]
    [HttpPost("CerrarSesion")] //8vo
    public async Task<IActionResult> CerrarSesion()
    {
        var userId = HttpContext.GetAuthenticatedUserId();
        if (!userId.HasValue)
        {
            return BadRequest(new { isSuccess = false, mensaje = "No fue posible determinar el usuario autenticado." });
        }

        var response = await _autorizacionService.CerrarSesion(userId.Value, HttpContext.RequestAborted);
        if (!response.IsSuccess)
        {
            return BadRequest(response);
        }

        _cookieService.EliminarCookiesDelUsuario();
        return Ok(response);
    }

    [Authorize]
    [HttpGet("Ping")] //9no (usar solo para PRUEBAS)
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