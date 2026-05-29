using SistemaEventos.Server.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//Inyección y Configuración de Dependencias
builder.Services.InicializarServidor(builder.Configuration, builder.Environment);

var app = builder.Build();

//Configuración e Inicialización de la Aplicación Web
app.UsarServidor(builder.Environment);

app.Run();