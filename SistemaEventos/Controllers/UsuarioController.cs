using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Negocio.Interfaz;
using Servicio.Interfaz;
using Transversal.DTOs;
using Transversal.Models;

namespace SistemaEventos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioBLL _usuario;
        private readonly IAutorizacionBLL _autorizacion;
        private readonly ICookieService _cookies;

        public UsuarioController(IConfiguration configuration, IUsuarioBLL usuarioBLL, IAutorizacionBLL autorizacionBLL, ICookieService cookies)
        {
            _configuration = configuration;
            _usuario = usuarioBLL;
            _autorizacion = autorizacionBLL;
            _cookies = cookies;
        }


        [HttpPost("RegistrarUsuario")] //1ro (Sign Up)
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroRequestDTO usuario)
        {
            var resultado = await _usuario.RegistrarUsuario(usuario);
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        [HttpGet("ConfirmarCuenta")] //2do (ejecutarlo mejor directamente desde el correo recibido)
        public async Task<IActionResult> ConfirmarCuenta(string guidAcceso)
        {
            Respuesta<UsuarioResponseDTO> resultado = await _usuario.ConfirmarCuenta(guidAcceso);
            return Redirect($"{_configuration.GetValue<string>("Frontend_URLs:Desarrollo")}/login?confirmacion={(resultado.IsSuccess ? "ok" : "error")}");
        }

        [HttpPost("AutenticarUsuario")] //3ro (Traditional Sign In)
        public async Task<IActionResult> AutenticarUsuario([FromBody] UsuarioLoginRequestDTO usuario)
        {
            var resultado = await _usuario.AutenticarUsuario(usuario);
            if (resultado.IsSuccess)
            {
                //_cookies.SetCookieAccessToken(resultado.Valor.AccessToken);
                _cookies.SetCookieRefreshToken(resultado.Valor.RefreshToken);

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
            else
            {
                return Ok(new
                {
                    isSuccess = resultado.IsSuccess,
                    mensaje = resultado.Mensaje
                });
            }
        }

        [HttpPost("AutenticarUsuarioGoogle")] //4to (Google Sign In)
        public async Task<IActionResult> AutenticarUsuarioGoogle([FromBody] UsuarioGoogleRequestDTO usuario)
        {
            var resultado = await _usuario.AutenticarUsuarioGoogle(usuario);
            if (resultado.IsSuccess)
            {
                _cookies.SetCookieRefreshToken(resultado.Valor.RefreshToken);
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
            else
            {
                return Ok(new { isSuccess = resultado.IsSuccess, mensaje = resultado.Mensaje });
            }
        }

        [HttpPost("RegistrarUsuarioGoogle")] //5to (Google Sign Up)
        public async Task<IActionResult> RegistrarUsuarioGoogle([FromBody] UsuarioGoogleRequestDTO usuario)
        {
            var resultado = await _usuario.RegistrarUsuarioGoogle(usuario);
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        [HttpPost("OlvidoSuContrasena")] //6to
        public async Task<IActionResult> OlvidoSuContrasena([FromBody] EmailDTO email)
        {
            var resultado = await _usuario.OlvidoSuContrasena(email.Email);
            return Ok(new
            {
                isSuccess = resultado.IsSuccess,
                mensaje = resultado.Mensaje
            });
        }

        [HttpPost("RestablecerContrasena")] //7mo
        public async Task<IActionResult> RestablecerContrasena([FromBody] ActualizarContrasenaDTO contrasena)
        {
            var resultado = await _usuario.ActualizarContrasenaAntigua(contrasena.GuidAcceso, contrasena.NuevaContrasena, contrasena.ConfirmacionContrasena);
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
            var idUsuario = HttpContext.Items["IdUsuario"]?.ToString();

            var response = await _autorizacion.CerrarSesion(int.Parse(idUsuario!));

            if (response.IsSuccess)
                return Ok(response);
            else
                return BadRequest(response);
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
}