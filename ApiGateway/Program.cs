using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Cargar configuración de Ocelot desde ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Registrar Ocelot en el contenedor de dependencias
builder.Services.AddOcelot();

// 3. Configurar CORS para permitir comunicación desde Angular (http://localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Activar CORS antes de Ocelot
app.UseCors("AllowFrontend");

// Endpoint informativo del Gateway
app.MapGet("/health", () => Results.Ok(new
{
    service = "ApiGateway",
    engine = "Ocelot",
    status = "Online",
    timestamp = DateTime.UtcNow
}));

// Activar Middleware de Ocelot
await app.UseOcelot();

app.Run();
