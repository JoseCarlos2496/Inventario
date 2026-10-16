using Inventario.Aplicacion.Command;
using Inventario.Aplicacion.Dto;
using Inventario.Aplicacion.Query;
using Inventario.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    /// <summary>
    /// Órdenes de compra que envía Recepción. Lo normal es que lleguen por el evento <c>recepcion.orden-compra.recibida</c>;
    /// este endpoint hace lo mismo por HTTP (pruebas o integración síncrona).
    /// </summary>
    [ApiController]
    [Route("api/inventario/ordenes")]
    public class OrdenesController : ControladorBase
    {
        private readonly ICustomMediator _mediator;

        public OrdenesController(ICustomMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Asigna (descuenta y avisa a Preparación) o rechaza con errores. Ambos casos responden 200; ver <c>data.estado</c>.</summary>
        [HttpPost]
        public async Task<IActionResult> ProcesarOrdenCompra([FromBody] ProcesarOrdenCompraDto dto, CancellationToken cancellationToken)
        {
            var asignacion = await _mediator.SendAsync(new ProcesarOrdenCompraCommand(null, dto.OrdenCompraId, dto.Lineas), cancellationToken);
            return Exito("ORD-S01", $"Orden de compra {asignacion.Estado.ToLowerInvariant()}.", asignacion);
        }

        [HttpGet("{ordenCompraId:guid}")]
        public async Task<IActionResult> ObtenerAsignacion(Guid ordenCompraId, CancellationToken cancellationToken)
        {
            var asignacion = await _mediator.SendAsync(new ObtenerAsignacionQuery(ordenCompraId), cancellationToken);
            return Exito("ORD-S02", "Orden de compra obtenida correctamente.", asignacion);
        }
    }
}
