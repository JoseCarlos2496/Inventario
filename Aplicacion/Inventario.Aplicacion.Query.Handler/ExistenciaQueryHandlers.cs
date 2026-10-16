using Inventario.Aplicacion.Dto;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Inventario.RepositorioContrato;

namespace Inventario.Aplicacion.Query.Handler
{
    public class ListarExistenciasQueryHandler : IRequestHandler<ListarExistenciasQuery, IReadOnlyList<ExistenciaDto>>
    {
        private readonly IRepositorioExistencia _repositorioExistencia;
        private readonly IRepositorioProducto _repositorioProducto;
        private readonly IRepositorioUbicacion _repositorioUbicacion;
        private readonly IRepositorioInventario _repositorioInventario;

        public ListarExistenciasQueryHandler(
            IRepositorioExistencia repositorioExistencia,
            IRepositorioProducto repositorioProducto,
            IRepositorioUbicacion repositorioUbicacion,
            IRepositorioInventario repositorioInventario)
        {
            _repositorioExistencia = repositorioExistencia;
            _repositorioProducto = repositorioProducto;
            _repositorioUbicacion = repositorioUbicacion;
            _repositorioInventario = repositorioInventario;
        }

        public async Task<IReadOnlyList<ExistenciaDto>> HandleAsync(ListarExistenciasQuery request, CancellationToken cancellationToken)
        {
            var productos = await _repositorioProducto.ListarAsync(cancellationToken);
            var existencias = (await _repositorioExistencia.ListarAsync(cancellationToken)).ToLookup(e => e.ProductoId);
            var ubicaciones = (await _repositorioUbicacion.ListarAsync(cancellationToken)).ToDictionary(u => u.Id);
            var inventarios = (await _repositorioInventario.ListarPorProductosAsync(productos.Select(p => p.Id).ToList(), cancellationToken))
                .ToDictionary(i => i.ProductoId);

            return productos
                .Select(p => p.ADto(existencias[p.Id], ubicaciones, inventarios.GetValueOrDefault(p.Id)))
                .ToList();
        }
    }

    public class ObtenerExistenciaQueryHandler : IRequestHandler<ObtenerExistenciaQuery, ExistenciaDto>
    {
        private readonly IRepositorioExistencia _repositorioExistencia;
        private readonly IRepositorioProducto _repositorioProducto;
        private readonly IRepositorioUbicacion _repositorioUbicacion;
        private readonly IRepositorioInventario _repositorioInventario;

        public ObtenerExistenciaQueryHandler(
            IRepositorioExistencia repositorioExistencia,
            IRepositorioProducto repositorioProducto,
            IRepositorioUbicacion repositorioUbicacion,
            IRepositorioInventario repositorioInventario)
        {
            _repositorioExistencia = repositorioExistencia;
            _repositorioProducto = repositorioProducto;
            _repositorioUbicacion = repositorioUbicacion;
            _repositorioInventario = repositorioInventario;
        }

        public async Task<ExistenciaDto> HandleAsync(ObtenerExistenciaQuery request, CancellationToken cancellationToken)
        {
            var producto = await _repositorioProducto.BuscarAsync(request.ProductoId, cancellationToken)
                ?? throw new ExcepcionNoEncontrado(CatalogoErrores.ProductoNoEncontrado, $"No existe el producto {request.ProductoId}.");

            var existencias = await _repositorioExistencia.ListarPorProductoAsync(request.ProductoId, cancellationToken);
            var ubicaciones = (await _repositorioUbicacion.ListarAsync(cancellationToken)).ToDictionary(u => u.Id);
            var inventario = await _repositorioInventario.BuscarPorProductoAsync(request.ProductoId, cancellationToken);
            return producto.ADto(existencias, ubicaciones, inventario);
        }
    }

    public class ListarMovimientosQueryHandler : IRequestHandler<ListarMovimientosQuery, IReadOnlyList<MovimientoDto>>
    {
        private readonly IRepositorioMovimiento _repositorioMovimiento;

        public ListarMovimientosQueryHandler(IRepositorioMovimiento repositorioMovimiento)
        {
            _repositorioMovimiento = repositorioMovimiento;
        }

        public async Task<IReadOnlyList<MovimientoDto>> HandleAsync(ListarMovimientosQuery request, CancellationToken cancellationToken)
        {
            var movimientos = await _repositorioMovimiento.ListarPorProductoAsync(request.ProductoId, cancellationToken);
            return movimientos.Select(m => m.ADto()).ToList();
        }
    }
}
