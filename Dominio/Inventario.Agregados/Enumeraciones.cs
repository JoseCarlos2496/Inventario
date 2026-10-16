namespace Inventario.Agregados
{
    public enum TipoMovimiento
    {
        Entrada,
        Salida
    }

    public enum TipoUbicacion
    {
        Bodega,
        Tienda,
        Otro
    }

    public enum EstadoAsignacion
    {
        /// <summary>Hubo stock para todo: se descontó y se avisó a Preparación de dónde recoger.</summary>
        Asignada,

        /// <summary>Faltó algo: no se descontó nada y se avisó el porqué (códigos ORD-1xx).</summary>
        Rechazada
    }
}
