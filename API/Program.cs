using SistemaEventos.Server.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//Inyección y Configuración de Dependencias
builder.Services.RegistroConfiguracionServicios(builder.Configuration);

//Configuración e Inicialización de la Aplicación Web
builder.Services.ConstruccionConfiguracionAppWeb(builder.Configuration, builder.Build());