using System.Diagnostics;
using System.Text.Json.Serialization;
using Inventario.Api.Configuracion;
using Inventario.Api.Respuestas;
using Inventario.Aplicacion.Command.Handler;
using Inventario.Aplicacion.Notification.Handler;
using Inventario.Aplicacion.Query.Handler;
using Inventario.Dominio.Comun;
using Inventario.Infraestructura.Mensajeria;
using Inventario.Infraestructura.Modelo;
using Inventario.Infraestructura.Repositorio;
using Inventario.MensajeriaContrato;
using Inventario.RepositorioContrato;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Persistencia: InMemory (por defecto, para arrancar sin nada instalado), SqlServer o PostgreSQL.
var proveedorBd = builder.Configuration["BaseDatos:Proveedor"] ?? "InMemory";
builder.Services.AddDbContext<ModeloInventario>(options =>
{
    switch (proveedorBd)
    {
        case "SqlServer":
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
            break;
        case "PostgreSQL":
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
            break;
        default:
            options.UseInMemoryDatabase("Inventario");
            break;
    }
});
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ModeloInventario>());

builder.Services.AddScoped<IRepositorioProducto, RepositorioProducto>();
builder.Services.AddScoped<IRepositorioUbicacion, RepositorioUbicacion>();
builder.Services.AddScoped<IRepositorioExistencia, RepositorioExistencia>();
builder.Services.AddScoped<IRepositorioAsignacion, RepositorioAsignacion>();
builder.Services.AddScoped<IRepositorioMovimiento, RepositorioMovimiento>();
builder.Services.AddScoped<IRepositorioInventario, RepositorioInventario>();
builder.Services.AddScoped<IRegistroEventosRecibidos, RegistroEventosRecibidos>();
builder.Services.AddScoped<IBandejaSalida, BandejaSalidaEf>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EvaluadorSaludInventario>();

builder.Services.AddCustomMediator(
    typeof(ProcesarOrdenCompraCommandHandler).Assembly,
    typeof(ObtenerAsignacionQueryHandler).Assembly,
    typeof(PublicarEventosIntegracionHandler).Assembly);

// Outbox + transporte (Log o RabbitMq según "Mensajeria:Proveedor").
builder.Services.AddMensajeriaInventario(builder.Configuration);

builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Mismas opciones JSON para las respuestas escritas fuera de MVC (p. ej. el manejador de excepciones).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = contexto =>
    {
        var errores = contexto.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new ErrorCampo(x.Key, e.ErrorMessage)))
            .ToList();

        var respuesta = new ApiResponse(false, CatalogoErrores.FormatoInvalido.Codigo, "La solicitud tiene datos inválidos.", DateTime.UtcNow,
            Activity.Current?.Id ?? contexto.HttpContext.TraceIdentifier, Errors: errores, Path: contexto.HttpContext.Request.Path);
        return new BadRequestObjectResult(respuesta);
    };
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Crea el esquema si no existe. Al pasar a una BD real conviene cambiarlo por migraciones (Database.Migrate()).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ModeloInventario>().Database.EnsureCreated();
    await DatosIniciales.SembrarUbicacionesAsync(scope.ServiceProvider);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
