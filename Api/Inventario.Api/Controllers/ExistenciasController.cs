using Inventario.Aplicacion.Command;
using Inventario.Aplicacion.Dto;
using Inventario.Aplicacion.Query;
using Inventario.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    [ApiController]
    [Route("api/inventario/existencias")]
    public class ExistenciasController : ControladorBase
    {
        private readonly ICustomMediator _mediator;

        public ExistenciasController(ICustomMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> ListarExistencias(CancellationToken cancellationToken)
        {
            var existencias = await _mediator.SendAsync(new ListarExistenciasQuery(), cancellationToken);
            return Exito("EXI-S01", "Existencias obtenidas correctamente.", existencias);
        }

        [HttpGet("{productoId:guid}")]
        public async Task<IActionResult> ObtenerExistencia(Guid productoId, CancellationToken cancellationToken)
        {
            var existencia = await _mediator.SendAsync(new ObtenerExistenciaQuery(productoId), cancellationToken);
            return Exito("EXI-S02", "Existencia obtenida correctamente.", existencia);
        }

        [HttpGet("{productoId:guid}/movimientos")]
        public async Task<IActionResult> ListarMovimientos(Guid productoId, CancellationToken cancellationToken)
        {
            var movimientos = await _mediator.SendAsync(new ListarMovimientosQuery(productoId), cancellationToken);
            return Exito("MOV-S01", "Movimientos obtenidos correctamente.", movimientos);
        }

        [HttpPut("{productoId:guid}/stock-minimo")]
        public async Task<IActionResult> DefinirStockMinimo(Guid productoId, [FromBody] DefinirStockMinimoDto dto, CancellationToken cancellationToken)
        {
            await _mediator.SendAsync(new DefinirStockMinimoCommand(productoId, dto.StockMinimo), cancellationToken);
            return Exito("INV-S01", "Stock mínimo actualizado correctamente.");
        }
    }
}
