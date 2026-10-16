using Inventario.Aplicacion.Dto;
using Inventario.Mediador;

namespace Inventario.Aplicacion.Query
{
    public sealed record ListarExistenciasQuery : IRequest<IReadOnlyList<ExistenciaDto>>;

    public sealed record ObtenerExistenciaQuery(Guid ProductoId) : IRequest<ExistenciaDto>;

    public sealed record ListarMovimientosQuery(Guid ProductoId) : IRequest<IReadOnlyList<MovimientoDto>>;

    public sealed record ListarProductosQuery : IRequest<IReadOnlyList<ProductoDto>>;

    public sealed record ObtenerProductoQuery(Guid ProductoId) : IRequest<ProductoDto>;

    public sealed record ListarUbicacionesQuery : IRequest<IReadOnlyList<UbicacionDto>>;

    public sealed record ObtenerAsignacionQuery(Guid OrdenCompraId) : IRequest<AsignacionDto>;
}
