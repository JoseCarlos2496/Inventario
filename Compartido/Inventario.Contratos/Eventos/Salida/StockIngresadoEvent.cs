namespace Inventario.Contratos.Eventos.Salida
{
    [EventoIntegracion("inventario.stock.ingresado", 1)]
    public sealed record StockIngresadoEvent(Guid ProductoId, Guid UbicacionId, int CantidadIngresada, int CantidadEnUbicacion, Guid CompraId) : IEventoIntegracion;
}
