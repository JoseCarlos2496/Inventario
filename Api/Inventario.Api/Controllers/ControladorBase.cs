using System.Diagnostics;
using Inventario.Api.Respuestas;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    public abstract class ControladorBase : ControllerBase
    {
        protected IActionResult Exito(string codigo, string mensaje, object? data = null, int status = StatusCodes.Status200OK)
        {
            var respuesta = new ApiResponse(true, codigo, mensaje, DateTime.UtcNow, Activity.Current?.Id ?? HttpContext.TraceIdentifier, data);
            return StatusCode(status, respuesta);
        }
    }
}
