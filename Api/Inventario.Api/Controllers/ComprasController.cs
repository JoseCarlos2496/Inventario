using Inventario.Aplicacion.Command;
using Inventario.Aplicacion.Dto;
using Inventario.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    /// <summary>
    /// Simula al microservicio de Compras mientras no exista: da de alta el producto con sus características
    /// e ingresa el stock en la ubicación (bodega, tienda, ...) de cada línea.
    /// </summary>
    [ApiController]
    [Route("api/inventario/compras")]
    public class ComprasController : ControladorBase
    {
        private readonly ICustomMediator _mediator;

        public ComprasController(ICustomMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("simular")]
        public async Task<IActionResult> SimularCompra([FromBody] SimularCompraDto dto, CancellationToken cancellationToken)
        {
            var compra = await _mediator.SendAsync(new SimularCompraCommand(dto.CompraId, dto.Lineas), cancellationToken);
            return Exito("COM-S01", "Compra registrada: productos creados/actualizados y stock ingresado.", compra, StatusCodes.Status201Created);
        }
    }
}
