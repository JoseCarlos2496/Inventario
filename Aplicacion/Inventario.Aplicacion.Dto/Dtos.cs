using Inventario.Agregados;

namespace Inventario.Aplicacion.Dto
{
    // ---- Entrada ----

    /// <summary>Línea de una compra simulada: crea/actualiza el producto por SKU e ingresa stock en la ubicación.</summary>
    public sealed record LineaCompraDto(
        string Sku,
        string Nombre,
        string? Descripcion,
        string? Marca,
        string? Modelo,
        string? Categoria,
        string? UnidadMedida,
        Dictionary<string, string>? Caracteristicas,
        string CodigoUbicacion,
        int Cantidad);

    public sealed record SimularCompraDto(Guid? CompraId, IReadOnlyList<LineaCompraDto> Lineas);

    public sealed record CrearUbicacionDto(string Codigo, string Nombre, TipoUbicacion Tipo, int Prioridad);

    public sealed record LineaOrdenCompraDto(Guid? ProductoId, string? Sku, int Cantidad);

    public sealed record ProcesarOrdenCompraDto(Guid OrdenCompraId, IReadOnlyList<LineaOrdenCompraDto> Lineas);

    public sealed record DefinirStockMinimoDto(int StockMinimo);

    // ---- Salida ----

    public sealed record LineaCompraResultadoDto(Guid ProductoId, string Sku, bool ProductoNuevo, string CodigoUbicacion, int CantidadIngresada, int CantidadEnUbicacion);

    public sealed record CompraSimuladaDto(Guid CompraId, IReadOnlyList<LineaCompraResultadoDto> Lineas);

    public sealed record UbicacionDto(Guid Id, string Codigo, string Nombre, string Tipo, int Prioridad);

    public sealed record ProductoDto(
        Guid Id,
        string Sku,
        string Nombre,
        string? Descripcion,
        string? Marca,
        string? Modelo,
        string? Categoria,
        string? UnidadMedida,
        IReadOnlyDictionary<string, string> Caracteristicas,
        int CantidadTotal);

    public sealed record ExistenciaUbicacionDto(Guid UbicacionId, string Codigo, string Nombre, string Tipo, int Cantidad);

    /// <summary>Stock de un producto: total y desglose por ubicación.</summary>
    public sealed record ExistenciaDto(
        Guid ProductoId,
        string Sku,
        string Nombre,
        int CantidadTotal,
        int StockMinimo,
        bool BajoMinimo,
        IReadOnlyList<ExistenciaUbicacionDto> Ubicaciones);

    public sealed record RecogidaDto(Guid UbicacionId, string Codigo, string Nombre, string Tipo, int Cantidad);

    public sealed record LineaAsignadaDto(Guid ProductoId, string Sku, string Nombre, int Cantidad, IReadOnlyList<RecogidaDto> Recoger);

    public sealed record ErrorOrdenDto(string Codigo, string Mensaje, Guid? ProductoId, string? Sku, int CantidadSolicitada, int CantidadDisponible);

    public sealed record AsignacionDto(
        Guid Id,
        Guid OrdenCompraId,
        string Estado,
        string? Motivo,
        IReadOnlyList<LineaAsignadaDto> Lineas,
        IReadOnlyList<ErrorOrdenDto> Errores);

    public sealed record MovimientoDto(Guid Id, Guid ProductoId, Guid UbicacionId, string Tipo, int Cantidad, Guid Referencia, DateTime Fecha);
}
