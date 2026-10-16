using Inventario.Agregados;
using Inventario.Aplicacion.Dto;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Inventario.ObjetosValor;
using Inventario.RepositorioContrato;

namespace Inventario.Aplicacion.Command.Handler
{
    public class ProcesarOrdenCompraCommandHandler : IRequestHandler<ProcesarOrdenCompraCommand, AsignacionDto>
    {
        private readonly IRepositorioAsignacion _repositorioAsignacion;
        private readonly IRepositorioProducto _repositorioProducto;
        private readonly IRepositorioExistencia _repositorioExistencia;
        private readonly IRepositorioUbicacion _repositorioUbicacion;
        private readonly IRepositorioMovimiento _repositorioMovimiento;
        private readonly IRegistroEventosRecibidos _eventosRecibidos;
        private readonly EvaluadorSaludInventario _evaluadorSalud;
        private readonly IUnitOfWork _unitOfWork;

        public ProcesarOrdenCompraCommandHandler(
            IRepositorioAsignacion repositorioAsignacion,
            IRepositorioProducto repositorioProducto,
            IRepositorioExistencia repositorioExistencia,
            IRepositorioUbicacion repositorioUbicacion,
            IRepositorioMovimiento repositorioMovimiento,
            IRegistroEventosRecibidos eventosRecibidos,
            EvaluadorSaludInventario evaluadorSalud,
            IUnitOfWork unitOfWork)
        {
            _repositorioAsignacion = repositorioAsignacion;
            _repositorioProducto = repositorioProducto;
            _repositorioExistencia = repositorioExistencia;
            _repositorioUbicacion = repositorioUbicacion;
            _repositorioMovimiento = repositorioMovimiento;
            _eventosRecibidos = eventosRecibidos;
            _evaluadorSalud = evaluadorSalud;
            _unitOfWork = unitOfWork;
        }

        public async Task<AsignacionDto> HandleAsync(ProcesarOrdenCompraCommand request, CancellationToken cancellationToken)
        {
            if (request.OrdenCompraId == Guid.Empty)
            {
                throw new ExcepcionValidacion(CatalogoErrores.OrdenSinIdentificador);
            }

            // Una asignación por orden: si ya se procesó (evento repetido o reintento) se devuelve el mismo resultado
            // y no se vuelve a descontar stock.
            var existente = await _repositorioAsignacion.BuscarPorOrdenAsync(request.OrdenCompraId, cancellationToken);
            if (existente is not null)
            {
                return existente.ADto();
            }

            if (request.EventoId is Guid eventoId)
            {
                _eventosRecibidos.Registrar(eventoId, nameof(ProcesarOrdenCompraCommand));
            }

            var lineas = (request.Lineas ?? [])
                .Select(l => new LineaSolicitada(l.ProductoId, l.Sku, l.Cantidad))
                .ToList();

            var productos = await BuscarProductosAsync(lineas, cancellationToken);
            var existencias = (await _repositorioExistencia.ListarPorProductosAsync(productos.Select(p => p.Id).ToList(), cancellationToken))
                .GroupBy(e => e.ProductoId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var ubicaciones = (await _repositorioUbicacion.ListarAsync(cancellationToken)).ToDictionary(u => u.Id);

            var asignacion = ServicioAsignacionStock.Asignar(request.OrdenCompraId, lineas, productos, existencias, ubicaciones);
            _repositorioAsignacion.Guardar(asignacion);

            if (asignacion.Estado == EstadoAsignacion.Asignada)
            {
                foreach (var detalle in asignacion.Detalles)
                {
                    _repositorioMovimiento.Guardar(MovimientoStock.Registrar(
                        detalle.ProductoId, detalle.UbicacionId, TipoMovimiento.Salida, Cantidad.Crear(detalle.Cantidad), request.OrdenCompraId));
                }

                foreach (var productoId in asignacion.Detalles.Select(d => d.ProductoId).Distinct())
                {
                    await _evaluadorSalud.EvaluarAsync(productoId, existencias[productoId].Sum(e => e.Cantidad), cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return asignacion.ADto();
        }

        private Task<List<Producto>> BuscarProductosAsync(IReadOnlyCollection<LineaSolicitada> lineas, CancellationToken cancellationToken)
        {
            var ids = lineas
                .Where(l => l.ProductoId is Guid id && id != Guid.Empty)
                .Select(l => l.ProductoId!.Value)
                .Distinct()
                .ToList();
            var skus = lineas
                .Where(l => !string.IsNullOrWhiteSpace(l.Sku))
                .Select(l => l.Sku!.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            return _repositorioProducto.ListarPorIdsOSkusAsync(ids, skus, cancellationToken);
        }
    }
}
