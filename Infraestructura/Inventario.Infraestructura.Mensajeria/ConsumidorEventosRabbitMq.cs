using System.Text.Json;
using Inventario.Contratos;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inventario.Infraestructura.Mensajeria
{
    /// <summary>
    /// Escucha la cola de Inventario (enlazada a los eventos de <see cref="ReceptorEventos.TiposConsumidos"/>).
    /// Errores técnicos: hasta 3 intentos (5 s, 10 s). Reglas de negocio incumplidas: no se reintentan.
    /// Lo que no se pudo procesar va a la cola de fallidos (dead letter).
    /// </summary>
    public sealed class ConsumidorEventosRabbitMq : BackgroundService
    {
        private static readonly TimeSpan EsperaReconexion = TimeSpan.FromSeconds(10);

        private readonly OpcionesRabbitMq _opciones;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ConsumidorEventosRabbitMq> _logger;

        public ConsumidorEventosRabbitMq(IOptions<OpcionesMensajeria> opciones, IServiceScopeFactory scopeFactory, ILogger<ConsumidorEventosRabbitMq> logger)
        {
            _opciones = opciones.Value.RabbitMq;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumirAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Sin conexión con RabbitMQ; reintento en {Espera}.", EsperaReconexion);
                }

                await Task.Delay(EsperaReconexion, stoppingToken);
            }
        }

        private async Task ConsumirAsync(CancellationToken stoppingToken)
        {
            await using var conexion = await FabricaConexionRabbitMq.Crear(_opciones).CreateConnectionAsync(stoppingToken);
            await using var canal = await conexion.CreateChannelAsync(cancellationToken: stoppingToken);

            await canal.ExchangeDeclareAsync(_opciones.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: stoppingToken);
            await canal.ExchangeDeclareAsync(_opciones.ExchangeFallidos, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: stoppingToken);
            await canal.QueueDeclareAsync(_opciones.ColaFallidos, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await canal.QueueBindAsync(_opciones.ColaFallidos, _opciones.ExchangeFallidos, string.Empty, cancellationToken: stoppingToken);

            var argumentos = new Dictionary<string, object?> { ["x-dead-letter-exchange"] = _opciones.ExchangeFallidos };
            await canal.QueueDeclareAsync(_opciones.Cola, durable: true, exclusive: false, autoDelete: false, arguments: argumentos, cancellationToken: stoppingToken);

            foreach (var tipo in ReceptorEventos.TiposConsumidos)
            {
                await canal.QueueBindAsync(_opciones.Cola, _opciones.Exchange, CatalogoEventos.Describir(tipo).Nombre, cancellationToken: stoppingToken);
            }

            await canal.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

            var consumidor = new AsyncEventingBasicConsumer(canal);
            consumidor.ReceivedAsync += async (_, entrega) =>
            {
                var procesado = await ProcesarConReintentosAsync(entrega.Body, entrega.RoutingKey, stoppingToken);
                if (procesado)
                {
                    await canal.BasicAckAsync(entrega.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                else
                {
                    await canal.BasicNackAsync(entrega.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await canal.BasicConsumeAsync(_opciones.Cola, autoAck: false, consumer: consumidor, cancellationToken: stoppingToken);
            _logger.LogInformation("Escuchando la cola {Cola} en {Exchange}.", _opciones.Cola, _opciones.Exchange);

            var cierre = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            conexion.ConnectionShutdownAsync += (_, _) =>
            {
                cierre.TrySetResult();
                return Task.CompletedTask;
            };

            await Task.WhenAny(cierre.Task, Task.Delay(Timeout.Infinite, stoppingToken));
            stoppingToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Se cerró la conexión con RabbitMQ.");
        }

        private async Task<bool> ProcesarConReintentosAsync(ReadOnlyMemory<byte> cuerpo, string routingKey, CancellationToken stoppingToken)
        {
            SobreEvento? sobre;
            try
            {
                sobre = JsonSerializer.Deserialize<SobreEvento>(cuerpo.Span, CatalogoEventos.OpcionesJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Mensaje con routing key {RoutingKey} no es un SobreEvento válido; se descarta a fallidos.", routingKey);
                return false;
            }

            if (sobre is null)
            {
                return false;
            }

            for (var intento = 1; ; intento++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var receptor = scope.ServiceProvider.GetRequiredService<ReceptorEventos>();
                    await receptor.ProcesarAsync(sobre, stoppingToken);
                    return true;
                }
                catch (ExcepcionDominio ex) when (ex.Codigo != CatalogoErrores.ConflictoConcurrencia.Codigo)
                {
                    _logger.LogWarning("Evento {Tipo}.v{Version} {EventoId} rechazado ({Codigo}): {Mensaje}",
                        sobre.Tipo, sobre.Version, sobre.EventoId, ex.Codigo, ex.Message);
                    return false;
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    if (intento >= PoliticaReintentos.MaxIntentos)
                    {
                        _logger.LogWarning(ex, "EVENTO FALLIDO {Tipo}.v{Version} {EventoId} tras {Intentos} intentos.",
                            sobre.Tipo, sobre.Version, sobre.EventoId, intento);
                        return false;
                    }

                    _logger.LogInformation(ex, "Intento {Intento} de {Tipo} {EventoId} falló; se reintenta.", intento, sobre.Tipo, sobre.EventoId);
                    await Task.Delay(PoliticaReintentos.Espera(intento), stoppingToken);
                }
            }
        }
    }
}
