using System.Text.Json;

namespace Inventario.Contratos
{
    /// <summary>
    /// Lo que viaja por el broker (o por HTTP): metadatos + payload JSON del evento.
    /// </summary>
    /// <param name="EventoId">Identificador único; sirve para deduplicar (la entrega es al menos una vez).</param>
    /// <param name="Tipo">Nombre estable, p. ej. <c>recepcion.orden-compra.recibida</c>.</param>
    /// <param name="Version">Versión del contrato.</param>
    /// <param name="OcurridoEn">Fecha UTC en que ocurrió.</param>
    /// <param name="Origen">Microservicio emisor.</param>
    /// <param name="Payload">Contenido del evento.</param>
    public sealed record SobreEvento(Guid EventoId, string Tipo, int Version, DateTime OcurridoEn, string Origen, JsonElement Payload);
}
