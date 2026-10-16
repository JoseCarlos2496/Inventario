using Inventario.Infraestructura.Modelo;
using Inventario.Mediador;
using Inventario.MensajeriaContrato;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Inventario.Infraestructura.Mensajeria
{
    /// <summary>
    /// Cada ~2 s toma los mensajes pendientes de la Outbox en orden y los publica. Máximo 3 intentos (esperas 5 s y 10 s);
    /// luego queda fallido, se escribe un WARNING y aparece en GET /api/sistema/eventos-fallidos.
    /// Pensado para una sola instancia: con varias, agregar bloqueo de filas (p. ej. FOR UPDATE SKIP LOCKED).
    /// </summary>
    public sealed class OutboxDispatcher : BackgroundService
    {
        private const int TamanoLote = 50;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPublicadorEventos _publicador;
        private readonly TimeSpan _intervalo;
        private readonly ILogger<OutboxDispatcher> _logger;

        public OutboxDispatcher(IServiceScopeFactory scopeFactory, IPublicadorEventos publicador, IOptions<OpcionesMensajeria> opciones, ILogger<OutboxDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _publicador = publicador;
            _intervalo = TimeSpan.FromSeconds(Math.Max(1, opciones.Value.IntervaloOutboxSegundos));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_intervalo);
            try
            {
                do
                {
                    try
                    {
                        await DespacharPendientesAsync(stoppingToken);
                    }
                    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogError(ex, "Error revisando la Outbox.");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task DespacharPendientesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var modelo = scope.ServiceProvider.GetRequiredService<ModeloInventario>();

            var ahora = DateTime.UtcNow;
            var pendientes = await modelo.Outbox
                .Where(m => m.ProcesadoEn == null && m.FallidoEn == null && (m.ProximoIntentoEn == null || m.ProximoIntentoEn <= ahora))
                .OrderBy(m => m.OcurridoEn)
                .Take(TamanoLote)
                .ToListAsync(cancellationToken);

            foreach (var mensaje in pendientes)
            {
                try
                {
                    await _publicador.PublicarAsync(mensaje.ASobre(), cancellationToken);
                    mensaje.MarcarProcesado();
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    var agotado = mensaje.RegistrarFallo(ex.Message, PoliticaReintentos.MaxIntentos, PoliticaReintentos.Espera(mensaje.Intentos + 1));
                    if (agotado)
                    {
                        _logger.LogWarning(ex, "EVENTO FALLIDO {Tipo}.v{Version} {EventoId} tras {Intentos} intentos.",
                            mensaje.Tipo, mensaje.Version, mensaje.Id, mensaje.Intentos);
                    }
                }

                await modelo.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
