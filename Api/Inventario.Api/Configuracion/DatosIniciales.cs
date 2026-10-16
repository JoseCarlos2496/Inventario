using Inventario.Agregados;
using Inventario.ObjetosValor;
using Inventario.RepositorioContrato;

namespace Inventario.Api.Configuracion
{
    /// <summary>
    /// Si la BD no tiene ubicaciones crea una bodega y una tienda, para poder simular compras y órdenes desde el arranque.
    /// </summary>
    public static class DatosIniciales
    {
        public static async Task SembrarUbicacionesAsync(IServiceProvider servicios)
        {
            var repositorio = servicios.GetRequiredService<IRepositorioUbicacion>();
            if (await repositorio.ExisteAlgunaAsync(CancellationToken.None))
            {
                return;
            }

            repositorio.Guardar(Ubicacion.Crear("BOD-01", Nombre.Crear("Bodega principal"), TipoUbicacion.Bodega, 1));
            repositorio.Guardar(Ubicacion.Crear("TDA-01", Nombre.Crear("Tienda"), TipoUbicacion.Tienda, 2));
            await servicios.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }
    }
}
