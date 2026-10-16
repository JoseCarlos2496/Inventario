namespace Inventario.Contratos.Eventos.Salida
{
    /// <summary>
    /// La orden de compra no se puede surtir completa; no se descontó nada. <c>Errores</c> trae cada motivo
    /// con su código del catálogo (<c>GET /api/inventario/errores/catalogo</c>, códigos ORD-1xx).
    /// </summary>
    [EventoIntegracion("inventario.orden.rechazada", 1)]
    public sealed record OrdenRechazadaEvent(Guid AsignacionId, Guid OrdenCompraId, string Motivo, IReadOnlyList<ErrorOrden> Errores) : IEventoIntegracion;

    public sealed record ErrorOrden(string Codigo, string Mensaje, Guid? ProductoId, string? Sku, int CantidadSolicitada, int CantidadDisponible);
}
