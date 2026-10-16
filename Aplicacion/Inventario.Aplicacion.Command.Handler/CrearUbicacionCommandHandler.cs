using Inventario.Agregados;
using Inventario.Aplicacion.Dto;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Inventario.ObjetosValor;
using Inventario.RepositorioContrato;

namespace Inventario.Aplicacion.Command.Handler
{
    public class CrearUbicacionCommandHandler : IRequestHandler<CrearUbicacionCommand, UbicacionDto>
    {
        private readonly IRepositorioUbicacion _repositorioUbicacion;
        private readonly IUnitOfWork _unitOfWork;

        public CrearUbicacionCommandHandler(IRepositorioUbicacion repositorioUbicacion, IUnitOfWork unitOfWork)
        {
            _repositorioUbicacion = repositorioUbicacion;
            _unitOfWork = unitOfWork;
        }

        public async Task<UbicacionDto> HandleAsync(CrearUbicacionCommand request, CancellationToken cancellationToken)
        {
            var codigo = Ubicacion.NormalizarCodigo(request.Codigo);
            if (await _repositorioUbicacion.BuscarPorCodigoAsync(codigo, cancellationToken) is not null)
            {
                throw new ExcepcionConflicto(CatalogoErrores.UbicacionDuplicada, $"Ya existe la ubicación {codigo}.");
            }

            var ubicacion = Ubicacion.Crear(codigo, Nombre.Crear(request.Nombre), request.Tipo, request.Prioridad);
            _repositorioUbicacion.Guardar(ubicacion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ubicacion.ADto();
        }
    }
}
