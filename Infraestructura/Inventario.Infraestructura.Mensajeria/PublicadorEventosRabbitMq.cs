using System.Text.Json;
using Inventario.Contratos;
using Inventario.MensajeriaContrato;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Inventario.Infraestructura.Mensajeria
{
    /// <summary>
    /// Publica el <see cref="SobreEvento"/> en el exchange topic usando el nombre estable como routing key.
    /// Usa confirmaciones del broker: si RabbitMQ no confirma, lanza excepción y la Outbox reintenta.
    /// </summary>
    public sealed class PublicadorEventosRabbitMq : IPublicadorEventos, IAsyncDisposable
    {
        private readonly OpcionesRabbitMq _opciones;
        private readonly SemaphoreSlim _candado = new(1, 1);
        private IConnection? _conexion;
        private IChannel? _canal;

        public PublicadorEventosRabbitMq(IOptions<OpcionesMensajeria> opciones)
        {
            _opciones = opciones.Value.RabbitMq;
        }

        public async Task PublicarAsync(SobreEvento sobre, CancellationToken cancellationToken)
        {
            await _candado.WaitAsync(cancellationToken);
            try
            {
                var canal = await ObtenerCanalAsync(cancellationToken);
                var cuerpo = JsonSerializer.SerializeToUtf8Bytes(sobre, CatalogoEventos.OpcionesJson);
                var propiedades = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    MessageId = sobre.EventoId.ToString(),
                    Type = $"{sobre.Tipo}.v{sobre.Version}",
                    AppId = sobre.Origen
                };

                await canal.BasicPublishAsync(_opciones.Exchange, sobre.Tipo, mandatory: false, basicProperties: propiedades, body: cuerpo, cancellationToken: cancellationToken);
            }
            catch
            {
                await CerrarAsync();
                throw;
            }
            finally
            {
                _candado.Release();
            }
        }

        private async Task<IChannel> ObtenerCanalAsync(CancellationToken cancellationToken)
        {
            if (_canal is { IsOpen: true })
            {
                return _canal;
            }

            await CerrarAsync();
            _conexion = await FabricaConexionRabbitMq.Crear(_opciones).CreateConnectionAsync(cancellationToken);
            _canal = await _conexion.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                cancellationToken);
            await _canal.ExchangeDeclareAsync(_opciones.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            return _canal;
        }

        private async Task CerrarAsync()
        {
            if (_canal is not null)
            {
                await _canal.DisposeAsync();
                _canal = null;
            }
            if (_conexion is not null)
            {
                await _conexion.DisposeAsync();
                _conexion = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await CerrarAsync();
            _candado.Dispose();
        }
    }
}
