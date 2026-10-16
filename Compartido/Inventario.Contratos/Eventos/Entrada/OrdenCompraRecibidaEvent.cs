namespace Inventario.Contratos.Eventos.Entrada
{
    /// <summary>
    /// Lo emite Recepción al recibir una orden de compra. Inventario la valida, ubica el stock (bodega, tienda, ...),
    /// lo descuenta y avisa a Preparación con <c>inventario.orden.asignada</c>; si no puede surtirla completa
    /// responde a Recepción con <c>inventario.orden.rechazada</c>.
    /// </summary>
    [EventoIntegracion("recepcion.orden-compra.recibida", 1)]
    public sealed record OrdenCompraRecibidaEvent(Guid OrdenCompraId, IReadOnlyList<LineaOrdenCompra> Lineas) : IEventoIntegracion;

    /// <summary>El producto se identifica por <c>ProductoId</c> o, si no viene, por <c>Sku</c>.</summary>
    public sealed record LineaOrdenCompra(Guid? ProductoId, string? Sku, int Cantidad);
}
