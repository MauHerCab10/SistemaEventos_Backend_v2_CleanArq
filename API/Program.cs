using SistemaEventos.Server.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServer(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseServer(builder.Environment);

app.Run();