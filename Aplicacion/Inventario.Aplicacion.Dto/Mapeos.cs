using Inventario.Agregados;

namespace Inventario.Aplicacion.Dto
{
    public static class Mapeos
    {
        public static AsignacionDto ADto(this AsignacionOrden asignacion)
        {
            var lineas = asignacion.Detalles
                .GroupBy(d => d.ProductoId)
                .Select(g => new LineaAsignadaDto(
                    g.Key,
                    g.First().Sku,
                    g.First().NombreProducto,
                    g.Sum(d => d.Cantidad),
                    g.Select(d => new RecogidaDto(d.UbicacionId, d.CodigoUbicacion, d.NombreUbicacion, d.TipoUbicacion.ToString(), d.Cantidad)).ToList()))
                .ToList();

            var errores = asignacion.Errores
                .Select(e => new ErrorOrdenDto(e.Codigo, e.Mensaje, e.ProductoId, e.Sku, e.CantidadSolicitada, e.CantidadDisponible))
                .ToList();

            return new AsignacionDto(asignacion.Id, asignacion.OrdenCompraId, asignacion.Estado.ToString(), asignacion.Motivo, lineas, errores);
        }

        public static UbicacionDto ADto(this Ubicacion ubicacion)
        {
            return new UbicacionDto(ubicacion.Id, ubicacion.Codigo, ubicacion.Nombre.Valor, ubicacion.Tipo.ToString(), ubicacion.Prioridad);
        }

        public static ProductoDto ADto(this Producto producto, int cantidadTotal)
        {
            return new ProductoDto(
                producto.Id,
                producto.Sku.Valor,
                producto.Nombre.Valor,
                producto.Descripcion,
                producto.Marca,
                producto.Modelo,
                producto.Categoria,
                producto.UnidadMedida,
                producto.Caracteristicas,
                cantidadTotal);
        }

        public static ExistenciaDto ADto(
            this Producto producto,
            IEnumerable<Existencia> existencias,
            IReadOnlyDictionary<Guid, Ubicacion> ubicaciones,
            InventarioProducto? inventario)
        {
            var porUbicacion = existencias
                .Where(e => ubicaciones.ContainsKey(e.UbicacionId))
                .Select(e => (Existencia: e, Ubicacion: ubicaciones[e.UbicacionId]))
                .OrderBy(x => x.Ubicacion.Prioridad)
                .ThenBy(x => x.Ubicacion.Codigo)
                .Select(x => new ExistenciaUbicacionDto(x.Ubicacion.Id, x.Ubicacion.Codigo, x.Ubicacion.Nombre.Valor, x.Ubicacion.Tipo.ToString(), x.Existencia.Cantidad))
                .ToList();

            return new ExistenciaDto(
                producto.Id,
                producto.Sku.Valor,
                producto.Nombre.Valor,
                porUbicacion.Sum(u => u.Cantidad),
                inventario?.StockMinimo ?? 0,
                inventario?.BajoMinimo ?? false,
                porUbicacion);
        }

        public static MovimientoDto ADto(this MovimientoStock movimiento)
        {
            return new MovimientoDto(movimiento.Id, movimiento.ProductoId, movimiento.UbicacionId, movimiento.Tipo.ToString(), movimiento.Cantidad, movimiento.Referencia, movimiento.Fecha);
        }
    }
}
