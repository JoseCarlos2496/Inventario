using System.Text.Json;
using Inventario.Contratos;

namespace Inventario.Infraestructura.Modelo
{
    /// <summary>
    /// Evento de integración pendiente de publicar. Se inserta en la misma transacción que los datos.
    /// </summary>
    public class MensajeOutbox
    {
        public const string Origen = "inventario";

        public Guid Id { get; private set; }

        public string Tipo { get; private set; } = null!;

        public int Version { get; private set; }

        public string Contenido { get; private set; } = null!;

        public DateTime OcurridoEn { get; private set; }

        public DateTime? ProcesadoEn { get; private set; }

        public DateTime? FallidoEn { get; private set; }

        public int Intentos { get; private set; }

        public string? UltimoError { get; private set; }

        public DateTime? ProximoIntentoEn { get; private set; }

        private MensajeOutbox() { }

        public static MensajeOutbox Desde(SobreEvento sobre)
        {
            return new MensajeOutbox
            {
                Id = sobre.EventoId,
                Tipo = sobre.Tipo,
                Version = sobre.Version,
                Contenido = sobre.Payload.GetRawText(),
                OcurridoEn = sobre.OcurridoEn
            };
        }

        public SobreEvento ASobre()
        {
            using var documento = JsonDocument.Parse(Contenido);
            return new SobreEvento(Id, Tipo, Version, OcurridoEn, Origen, documento.RootElement.Clone());
        }

        public void MarcarProcesado()
        {
            ProcesadoEn = DateTime.UtcNow;
            UltimoError = null;
        }

        /// <summary>
        /// Registra el fallo. Devuelve true si se agotaron los intentos y el mensaje quedó fallido.
        /// </summary>
        public bool RegistrarFallo(string error, int maxIntentos, TimeSpan espera)
        {
            Intentos++;
            UltimoError = error.Length > 2000 ? error[..2000] : error;
            if (Intentos >= maxIntentos)
            {
                FallidoEn = DateTime.UtcNow;
                return true;
            }
            ProximoIntentoEn = DateTime.UtcNow.Add(espera);
            return false;
        }

        public void Reencolar()
        {
            FallidoEn = null;
            Intentos = 0;
            ProximoIntentoEn = null;
        }
    }
}
