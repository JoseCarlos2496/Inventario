using System.Diagnostics;
using Inventario.Dominio.Comun;
using Microsoft.AspNetCore.Diagnostics;

namespace Inventario.Api.Respuestas
{
    /// <summary>
    /// Traduce excepciones a <see cref="ApiResponse"/>: Validación 400 · No encontrado 404 · Conflicto 409 · resto 500.
    /// </summary>
    public sealed class ManejadorExcepciones : IExceptionHandler
    {
        private readonly ILogger<ManejadorExcepciones> _logger;

        public ManejadorExcepciones(ILogger<ManejadorExcepciones> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, codigo, mensaje) = exception switch
            {
                ExcepcionValidacion e => (StatusCodes.Status400BadRequest, e.Codigo, e.Message),
                ExcepcionNoEncontrado e => (StatusCodes.Status404NotFound, e.Codigo, e.Message),
                ExcepcionConflicto e => (StatusCodes.Status409Conflict, e.Codigo, e.Message),
                _ => (StatusCodes.Status500InternalServerError, CatalogoErrores.ErrorInesperado.Codigo, "Ocurrió un error inesperado.")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Error no controlado en {Ruta}", httpContext.Request.Path);
            }

            var respuesta = new ApiResponse(
                false,
                codigo,
                mensaje,
                DateTime.UtcNow,
                Activity.Current?.Id ?? httpContext.TraceIdentifier,
                Path: httpContext.Request.Path);

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(respuesta, cancellationToken);
            return true;
        }
    }
}
