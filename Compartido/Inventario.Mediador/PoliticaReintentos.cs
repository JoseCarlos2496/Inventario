namespace Inventario.Mediador
{
    /// <summary>
    /// Política única de reintentos: ninguna operación automática (Outbox, consumo de eventos) se reintenta más de
    /// <see cref="MaxIntentos"/> veces.
    /// </summary>
    public static class PoliticaReintentos
    {
        public const int MaxIntentos = 3;

        /// <summary>Espera antes del siguiente intento (lineal: 5 s, 10 s...).</summary>
        public static TimeSpan Espera(int intentosHechos) => TimeSpan.FromSeconds(5 * intentosHechos);
    }
}
