namespace Inventario.Contratos.Eventos.Salida
{
    [EventoIntegracion("inventario.stock.bajo-minimo", 1)]
    public sealed record StockBajoMinimoEvent(Guid ProductoId, int CantidadDisponible, int StockMinimo) : IEventoIntegracion;
}
