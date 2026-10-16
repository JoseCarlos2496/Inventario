namespace Inventario.Dominio.Eventos
{
    /// <summary>Recoger <c>Cantidad</c> unidades del producto en la ubicación indicada.</summary>
    public sealed record RecogidaEvento(
        Guid ProductoId,
        string Sku,
        string NombreProducto,
        Guid UbicacionId,
        string CodigoUbicacion,
        string NombreUbicacion,
        string TipoUbicacion,
        int Cantidad);

    public sealed record ErrorOrdenEvento(string Codigo, string Mensaje, Guid? ProductoId, string? Sku, int CantidadSolicitada, int CantidadDisponible);

    public sealed record StockIngresadoDomainEvent(Guid ProductoId, Guid UbicacionId, int CantidadIngresada, int CantidadEnUbicacion, Guid CompraId) : EventoDominio;

    public sealed record OrdenAsignadaDomainEvent(Guid AsignacionId, Guid OrdenCompraId, IReadOnlyList<RecogidaEvento> Recogidas) : EventoDominio;

    public sealed record OrdenRechazadaDomainEvent(Guid AsignacionId, Guid OrdenCompraId, string Motivo, IReadOnlyList<ErrorOrdenEvento> Errores) : EventoDominio;

    public sealed record StockBajoMinimoDomainEvent(Guid ProductoId, int CantidadDisponible, int StockMinimo) : EventoDominio;
}
