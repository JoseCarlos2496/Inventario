using Inventario.Aplicacion.Dto;
using Inventario.Mediador;

namespace Inventario.Aplicacion.Command
{
    // EventoId: presente cuando el comando nace de un evento externo; se usa para no aplicarlo dos veces.

    /// <summary>
    /// Recepción recibió una orden de compra: validar, ubicar el stock por ubicación, descontarlo y avisar a Preparación
    /// (o rechazarla con el porqué).
    /// </summary>
    public sealed record ProcesarOrdenCompraCommand(Guid? EventoId, Guid OrdenCompraId, IReadOnlyList<LineaOrdenCompraDto> Lineas) : IRequest<AsignacionDto>;

    /// <summary>
    /// Simula a Compras (que aún no existe): crea o actualiza productos por SKU e ingresa stock en cada ubicación.
    /// </summary>
    public sealed record SimularCompraCommand(Guid? CompraId, IReadOnlyList<LineaCompraDto> Lineas) : IRequest<CompraSimuladaDto>;

    public sealed record CrearUbicacionCommand(string Codigo, string Nombre, Agregados.TipoUbicacion Tipo, int Prioridad) : IRequest<UbicacionDto>;

    public sealed record DefinirStockMinimoCommand(Guid ProductoId, int StockMinimo) : IRequest<Unidad>;
}
