using Inventario.Infraestructura.Mensajeria;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    [ApiController]
    [Route("api/sistema")]
    public class SistemaController : ControladorBase
    {
        private readonly AdministracionOutbox _administracionOutbox;

        public SistemaController(AdministracionOutbox administracionOutbox)
        {
            _administracionOutbox = administracionOutbox;
        }

        [HttpGet("eventos-fallidos")]
        public async Task<IActionResult> ListarEventosFallidos(CancellationToken cancellationToken)
        {
            var fallidos = await _administracionOutbox.ListarFallidosAsync(cancellationToken);
            return Exito("SYS-S01", "Eventos fallidos obtenidos correctamente.", fallidos);
        }

        [HttpPut("eventos-fallidos/{id:guid}/reintentar")]
        public async Task<IActionResult> ReintentarEvento(Guid id, CancellationToken cancellationToken)
        {
            await _administracionOutbox.ReintentarAsync(id, cancellationToken);
            return Exito("SYS-S02", "Evento reencolado; se publicará en el próximo ciclo.");
        }
    }
}
