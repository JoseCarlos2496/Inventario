using Inventario.Aplicacion.Command;
using Inventario.Aplicacion.Dto;
using Inventario.Aplicacion.Query;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers
{
    [ApiController]
    [Route("api/inventario")]
    public class CatalogoController : ControladorBase
    {
        private readonly ICustomMediator _mediator;

        public CatalogoController(ICustomMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("productos")]
        public async Task<IActionResult> ListarProductos(CancellationToken cancellationToken)
        {
            var productos = await _mediator.SendAsync(new ListarProductosQuery(), cancellationToken);
            return Exito("PRO-S01", "Productos obtenidos correctamente.", productos);
        }

        [HttpGet("productos/{productoId:guid}")]
        public async Task<IActionResult> ObtenerProducto(Guid productoId, CancellationToken cancellationToken)
        {
            var producto = await _mediator.SendAsync(new ObtenerProductoQuery(productoId), cancellationToken);
            return Exito("PRO-S02", "Producto obtenido correctamente.", producto);
        }

        [HttpGet("ubicaciones")]
        public async Task<IActionResult> ListarUbicaciones(CancellationToken cancellationToken)
        {
            var ubicaciones = await _mediator.SendAsync(new ListarUbicacionesQuery(), cancellationToken);
            return Exito("UBI-S01", "Ubicaciones obtenidas correctamente.", ubicaciones);
        }

        [HttpPost("ubicaciones")]
        public async Task<IActionResult> CrearUbicacion([FromBody] CrearUbicacionDto dto, CancellationToken cancellationToken)
        {
            var ubicacion = await _mediator.SendAsync(new CrearUbicacionCommand(dto.Codigo, dto.Nombre, dto.Tipo, dto.Prioridad), cancellationToken);
            return Exito("UBI-S02", "Ubicación creada correctamente.", ubicacion, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Catálogo de códigos de error. <c>tipo = Rechazo</c> son los motivos que viajan en <c>inventario.orden.rechazada</c>.
        /// </summary>
        [HttpGet("errores/catalogo")]
        public IActionResult ObtenerCatalogoErrores()
        {
            var catalogo = CatalogoErrores.Todos.Select(e => new
            {
                codigo = e.Codigo,
                tipo = e.Tipo.ToString(),
                http = Http(e.Tipo),
                descripcion = e.Descripcion
            });
            return Exito("ERR-S01", "Catálogo de errores.", catalogo);
        }

        private static int? Http(TipoError tipo) => tipo switch
        {
            TipoError.Validacion => StatusCodes.Status400BadRequest,
            TipoError.NoEncontrado => StatusCodes.Status404NotFound,
            TipoError.Conflicto => StatusCodes.Status409Conflict,
            TipoError.Sistema => StatusCodes.Status500InternalServerError,
            _ => null
        };
    }
}
