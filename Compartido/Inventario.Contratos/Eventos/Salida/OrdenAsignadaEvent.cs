namespace Inventario.Contratos.Eventos.Salida
{
    /// <summary>
    /// La orden de compra se puede surtir completa: el stock ya se descontó. Le dice a Preparación qué recoger y en qué ubicación.
    /// </summary>
    [EventoIntegracion("inventario.orden.asignada", 1)]
    public sealed record OrdenAsignadaEvent(Guid AsignacionId, Guid OrdenCompraId, IReadOnlyList<LineaPreparacion> Lineas) : IEventoIntegracion;

    /// <param name="Cantidad">Total del producto en la orden (= suma de <paramref name="Recoger"/>).</param>
    public sealed record LineaPreparacion(Guid ProductoId, string Sku, string Nombre, int Cantidad, IReadOnlyList<PuntoRecogida> Recoger);

    /// <param name="TipoUbicacion"><c>Bodega</c>, <c>Tienda</c> u <c>Otro</c>.</param>
    public sealed record PuntoRecogida(Guid UbicacionId, string CodigoUbicacion, string NombreUbicacion, string TipoUbicacion, int Cantidad);
}
