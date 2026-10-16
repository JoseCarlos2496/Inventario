using Inventario.Agregados;
using Inventario.RepositorioContrato;
using Inventario.Mediador;

namespace Inventario.Aplicacion.Command.Handler
{
    public class DefinirStockMinimoCommandHandler : IRequestHandler<DefinirStockMinimoCommand, Unidad>
    {
        private readonly IRepositorioInventario _repositorioInventario;
        private readonly IRepositorioExistencia _repositorioExistencia;
        private readonly IUnitOfWork _unitOfWork;

        public DefinirStockMinimoCommandHandler(IRepositorioInventario repositorioInventario, IRepositorioExistencia repositorioExistencia, IUnitOfWork unitOfWork)
        {
            _repositorioInventario = repositorioInventario;
            _repositorioExistencia = repositorioExistencia;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unidad> HandleAsync(DefinirStockMinimoCommand request, CancellationToken cancellationToken)
        {
            var inventario = await _repositorioInventario.BuscarPorProductoAsync(request.ProductoId, cancellationToken);
            if (inventario is null)
            {
                inventario = InventarioProducto.Crear(request.ProductoId, request.StockMinimo);
                _repositorioInventario.Guardar(inventario);
            }
            else
            {
                inventario.DefinirStockMinimo(request.StockMinimo);
            }

            var existencias = await _repositorioExistencia.ListarPorProductoAsync(request.ProductoId, cancellationToken);
            inventario.EvaluarSalud(existencias.Sum(e => e.Cantidad));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unidad.Valor;
        }
    }
}
