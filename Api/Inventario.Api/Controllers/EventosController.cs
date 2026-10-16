using Inventario.Contratos;
using Inventario.Infraestructura.Mensajeria;
using Inventario.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    /// <summary>
    /// Entrada de eventos por HTTP (webhook). Sirve para simular a Recepción mientras no exista el broker,
    /// y como alternativa si otro equipo prefiere notificar por HTTP. Mismo sobre y misma idempotencia que por RabbitMQ.
    /// </summary>
    [ApiController]
    [Route("api/inventario/eventos")]
    public class EventosController : ControladorBase
    {
        private readonly ReceptorEventos _receptor;

        public EventosController(ReceptorEventos receptor)
        {
            _receptor = receptor;
        }

        [HttpPost]
        public async Task<IActionResult> RecibirEvento([FromBody] SobreEvento sobre, CancellationToken cancellationToken)
        {
            var resultado = await _receptor.ProcesarAsync(sobre, cancellationToken);
            return Exito("EVT-S01", $"Evento {sobre.Tipo}.v{sobre.Version} procesado.", resultado is Unidad ? null : resultado);
        }

        /// <summary>
        /// Catálogo de eventos que Inventario consume y publica (contrato para los demás equipos).
        /// </summary>
        [HttpGet("catalogo")]
        public IActionResult ObtenerCatalogo()
        {
            var catalogo = CatalogoEventos.Todos
                .Select(x => new
                {
                    nombre = x.Value.Nombre,
                    version = x.Value.Version,
                    direccion = ReceptorEventos.TiposConsumidos.Contains(x.Key) ? "consume" : "publica",
                    clase = x.Key.Name
                })
                .OrderBy(x => x.direccion)
                .ThenBy(x => x.nombre);

            return Exito("EVT-S02", "Catálogo de eventos.", catalogo);
        }
    }
}
