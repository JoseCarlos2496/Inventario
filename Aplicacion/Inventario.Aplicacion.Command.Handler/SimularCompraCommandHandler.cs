using Inventario.Agregados;
using Inventario.Aplicacion.Dto;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Inventario.ObjetosValor;
using Inventario.RepositorioContrato;

namespace Inventario.Aplicacion.Command.Handler
{
    /// <summary>
    /// Hace las veces de Compras: alta/actualización del producto (por SKU) e ingreso de stock en la ubicación indicada.
    /// </summary>
    public class SimularCompraCommandHandler : IRequestHandler<SimularCompraCommand, CompraSimuladaDto>
    {
        private readonly IRepositorioProducto _repositorioProducto;
        private readonly IRepositorioExistencia _repositorioExistencia;
        private readonly IRepositorioUbicacion _repositorioUbicacion;
        private readonly IRepositorioMovimiento _repositorioMovimiento;
        private readonly IRegistroEventosRecibidos _eventosRecibidos;
        private readonly EvaluadorSaludInventario _evaluadorSalud;
        private readonly IUnitOfWork _unitOfWork;

        public SimularCompraCommandHandler(
            IRepositorioProducto repositorioProducto,
            IRepositorioExistencia repositorioExistencia,
            IRepositorioUbicacion repositorioUbicacion,
            IRepositorioMovimiento repositorioMovimiento,
            IRegistroEventosRecibidos eventosRecibidos,
            EvaluadorSaludInventario evaluadorSalud,
            IUnitOfWork unitOfWork)
        {
            _repositorioProducto = repositorioProducto;
            _repositorioExistencia = repositorioExistencia;
            _repositorioUbicacion = repositorioUbicacion;
            _repositorioMovimiento = repositorioMovimiento;
            _eventosRecibidos = eventosRecibidos;
            _evaluadorSalud = evaluadorSalud;
            _unitOfWork = unitOfWork;
        }

        public async Task<CompraSimuladaDto> HandleAsync(SimularCompraCommand request, CancellationToken cancellationToken)
        {
            if (request.Lineas is null || request.Lineas.Count == 0)
            {
                throw new ExcepcionValidacion(CatalogoErrores.CompraSinLineas);
            }

            // Reenviar la misma compra no debe duplicar el stock.
            var compraId = request.CompraId ?? Guid.NewGuid();
            if (await _eventosRecibidos.FueProcesadoAsync(compraId, cancellationToken))
            {
                throw new ExcepcionConflicto(CatalogoErrores.CompraDuplicada, $"La compra {compraId} ya fue registrada.");
            }
            _eventosRecibidos.Registrar(compraId, nameof(SimularCompraCommand));

            var ubicaciones = (await _repositorioUbicacion.ListarAsync(cancellationToken)).ToDictionary(u => u.Codigo);
            var resultados = new List<LineaCompraResultadoDto>();

            foreach (var grupo in request.Lineas.GroupBy(l => Sku.Crear(l.Sku).Valor))
            {
                var (producto, nuevo) = await RegistrarProductoAsync(grupo.Key, grupo.ToList(), cancellationToken);
                var existencias = nuevo ? [] : await _repositorioExistencia.ListarPorProductoAsync(producto.Id, cancellationToken);

                foreach (var linea in grupo)
                {
                    var codigo = Ubicacion.NormalizarCodigo(linea.CodigoUbicacion);
                    var ubicacion = ubicaciones.GetValueOrDefault(codigo)
                        ?? throw new ExcepcionNoEncontrado(CatalogoErrores.UbicacionNoEncontrada, $"No existe la ubicación {codigo}.");

                    var existencia = existencias.FirstOrDefault(e => e.UbicacionId == ubicacion.Id);
                    if (existencia is null)
                    {
                        existencia = Existencia.Crear(producto.Id, ubicacion.Id);
                        existencias.Add(existencia);
                        _repositorioExistencia.Guardar(existencia);
                    }

                    var cantidad = Cantidad.Crear(linea.Cantidad);
                    existencia.Ingresar(cantidad, compraId);
                    _repositorioMovimiento.Guardar(MovimientoStock.Registrar(producto.Id, ubicacion.Id, TipoMovimiento.Entrada, cantidad, compraId));
                    resultados.Add(new LineaCompraResultadoDto(producto.Id, producto.Sku.Valor, nuevo, ubicacion.Codigo, cantidad.Valor, existencia.Cantidad));
                }

                await _evaluadorSalud.EvaluarAsync(producto.Id, existencias.Sum(e => e.Cantidad), cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new CompraSimuladaDto(compraId, resultados);
        }

        private async Task<(Producto Producto, bool Nuevo)> RegistrarProductoAsync(string sku, IReadOnlyList<LineaCompraDto> lineas, CancellationToken cancellationToken)
        {
            var producto = await _repositorioProducto.BuscarPorSkuAsync(sku, cancellationToken);
            var nuevo = producto is null;

            // Si el SKU llega en varias líneas (p. ej. una por ubicación), se aplican sus datos en orden.
            foreach (var linea in lineas)
            {
                var nombre = Nombre.Crear(linea.Nombre);
                var datos = new DatosProducto(linea.Descripcion, linea.Marca, linea.Modelo, linea.Categoria, linea.UnidadMedida, linea.Caracteristicas);
                if (producto is null)
                {
                    producto = Producto.Crear(Sku.Crear(sku), nombre, datos);
                    _repositorioProducto.Guardar(producto);
                }
                else
                {
                    producto.Actualizar(nombre, datos);
                }
            }

            return (producto!, nuevo);
        }
    }
}
