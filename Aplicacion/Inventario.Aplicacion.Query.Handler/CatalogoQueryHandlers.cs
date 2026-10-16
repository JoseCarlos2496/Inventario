using Inventario.Aplicacion.Dto;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Inventario.RepositorioContrato;

namespace Inventario.Aplicacion.Query.Handler
{
    public class ListarProductosQueryHandler : IRequestHandler<ListarProductosQuery, IReadOnlyList<ProductoDto>>
    {
        private readonly IRepositorioProducto _repositorioProducto;
        private readonly IRepositorioExistencia _repositorioExistencia;

        public ListarProductosQueryHandler(IRepositorioProducto repositorioProducto, IRepositorioExistencia repositorioExistencia)
        {
            _repositorioProducto = repositorioProducto;
            _repositorioExistencia = repositorioExistencia;
        }

        public async Task<IReadOnlyList<ProductoDto>> HandleAsync(ListarProductosQuery request, CancellationToken cancellationToken)
        {
            var productos = await _repositorioProducto.ListarAsync(cancellationToken);
            var totales = (await _repositorioExistencia.ListarAsync(cancellationToken))
                .GroupBy(e => e.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Cantidad));

            return productos.Select(p => p.ADto(totales.GetValueOrDefault(p.Id))).ToList();
        }
    }

    public class ObtenerProductoQueryHandler : IRequestHandler<ObtenerProductoQuery, ProductoDto>
    {
        private readonly IRepositorioProducto _repositorioProducto;
        private readonly IRepositorioExistencia _repositorioExistencia;

        public ObtenerProductoQueryHandler(IRepositorioProducto repositorioProducto, IRepositorioExistencia repositorioExistencia)
        {
            _repositorioProducto = repositorioProducto;
            _repositorioExistencia = repositorioExistencia;
        }

        public async Task<ProductoDto> HandleAsync(ObtenerProductoQuery request, CancellationToken cancellationToken)
        {
            var producto = await _repositorioProducto.BuscarAsync(request.ProductoId, cancellationToken)
                ?? throw new ExcepcionNoEncontrado(CatalogoErrores.ProductoNoEncontrado, $"No existe el producto {request.ProductoId}.");

            var existencias = await _repositorioExistencia.ListarPorProductoAsync(request.ProductoId, cancellationToken);
            return producto.ADto(existencias.Sum(e => e.Cantidad));
        }
    }

    public class ListarUbicacionesQueryHandler : IRequestHandler<ListarUbicacionesQuery, IReadOnlyList<UbicacionDto>>
    {
        private readonly IRepositorioUbicacion _repositorioUbicacion;

        public ListarUbicacionesQueryHandler(IRepositorioUbicacion repositorioUbicacion)
        {
            _repositorioUbicacion = repositorioUbicacion;
        }

        public async Task<IReadOnlyList<UbicacionDto>> HandleAsync(ListarUbicacionesQuery request, CancellationToken cancellationToken)
        {
            var ubicaciones = await _repositorioUbicacion.ListarAsync(cancellationToken);
            return ubicaciones.OrderBy(u => u.Prioridad).ThenBy(u => u.Codigo).Select(u => u.ADto()).ToList();
        }
    }

    public class ObtenerAsignacionQueryHandler : IRequestHandler<ObtenerAsignacionQuery, AsignacionDto>
    {
        private readonly IRepositorioAsignacion _repositorioAsignacion;

        public ObtenerAsignacionQueryHandler(IRepositorioAsignacion repositorioAsignacion)
        {
            _repositorioAsignacion = repositorioAsignacion;
        }

        public async Task<AsignacionDto> HandleAsync(ObtenerAsignacionQuery request, CancellationToken cancellationToken)
        {
            var asignacion = await _repositorioAsignacion.BuscarPorOrdenAsync(request.OrdenCompraId, cancellationToken)
                ?? throw new ExcepcionNoEncontrado(CatalogoErrores.OrdenNoProcesada, $"Inventario no ha procesado la orden de compra {request.OrdenCompraId}.");
            return asignacion.ADto();
        }
    }
}
