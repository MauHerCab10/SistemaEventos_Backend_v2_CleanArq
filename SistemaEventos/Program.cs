using SistemaEventos.IoC;

var builder = WebApplication.CreateBuilder(args);

//Inyección y Configuración de Dependencias
builder.Services.InyectarDependencias(builder.Configuration);

//Configuración e Inicialización de la Aplicación Web
builder.Services.ConfigurarInicializacionAplicacionWeb(builder.Configuration, builder.Build());