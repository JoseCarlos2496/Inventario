using Inventario.Dominio.Comun;
using Inventario.ObjetosValor;

namespace Inventario.Agregados
{
    /// <summary>Línea tal como llega en la orden de compra (aún sin validar). Se identifica por productoId o por sku.</summary>
    public sealed record LineaSolicitada(Guid? ProductoId, string? Sku, int Cantidad);

    /// <summary>
    /// Servicio de dominio: valida la orden completa y, solo si todo alcanza, descuenta stock repartiéndolo entre ubicaciones.
    /// Abarca varios productos, varias <see cref="Existencia"/> y una <see cref="AsignacionOrden"/>.
    /// </summary>
    public static class ServicioAsignacionStock
    {
        /// <param name="productos">Productos que coinciden con las líneas (por id o sku).</param>
        /// <param name="existenciasPorProducto">Existencias (en seguimiento) de esos productos en todas las ubicaciones.</param>
        /// <param name="ubicaciones">Ubicaciones registradas, por id.</param>
        public static AsignacionOrden Asignar(
            Guid ordenCompraId,
            IReadOnlyCollection<LineaSolicitada> lineas,
            IReadOnlyCollection<Producto> productos,
            IReadOnlyDictionary<Guid, List<Existencia>> existenciasPorProducto,
            IReadOnlyDictionary<Guid, Ubicacion> ubicaciones)
        {
            var errores = new List<AsignacionError>();

            if (lineas.Count == 0)
            {
                errores.Add(new AsignacionError(CatalogoErrores.OrdenSinLineas, CatalogoErrores.OrdenSinLineas.Descripcion, null, null, 0, 0));
                return AsignacionOrden.Rechazar(ordenCompraId, errores);
            }

            // 1. Cada línea debe identificar un producto registrado y pedir una cantidad válida.
            var solicitado = new Dictionary<Guid, (Producto Producto, int Cantidad)>();
            foreach (var linea in lineas)
            {
                var sku = string.IsNullOrWhiteSpace(linea.Sku) ? null : linea.Sku.Trim().ToUpperInvariant();
                var tieneId = linea.ProductoId is Guid id && id != Guid.Empty;

                if (!tieneId && sku is null)
                {
                    errores.Add(new AsignacionError(CatalogoErrores.LineaSinProducto, CatalogoErrores.LineaSinProducto.Descripcion, null, null, linea.Cantidad, 0));
                    continue;
                }

                var producto = tieneId
                    ? productos.FirstOrDefault(p => p.Id == linea.ProductoId)
                    : productos.FirstOrDefault(p => p.Sku.Valor == sku);

                if (linea.Cantidad <= 0)
                {
                    errores.Add(new AsignacionError(CatalogoErrores.CantidadSolicitadaInvalida,
                        $"Cantidad inválida ({linea.Cantidad}) para {Describir(producto, linea.ProductoId, sku)}.",
                        producto?.Id ?? linea.ProductoId, producto?.Sku.Valor ?? sku, linea.Cantidad, 0));
                    continue;
                }

                if (producto is null)
                {
                    errores.Add(new AsignacionError(CatalogoErrores.ProductoNoRegistrado,
                        $"El producto {Describir(null, linea.ProductoId, sku)} no está registrado en Inventario.",
                        linea.ProductoId, sku, linea.Cantidad, 0));
                    continue;
                }

                // La misma orden puede repetir el producto en varias líneas: se suma.
                var previo = solicitado.GetValueOrDefault(producto.Id);
                solicitado[producto.Id] = (producto, previo.Cantidad + linea.Cantidad);
            }

            // 2. La suma de todas las ubicaciones debe alcanzar lo pedido.
            foreach (var (productoId, (producto, cantidad)) in solicitado)
            {
                var existencias = Ordenar(existenciasPorProducto.GetValueOrDefault(productoId) ?? [], ubicaciones);
                var disponible = existencias.Sum(e => e.Cantidad);

                if (disponible == 0)
                {
                    errores.Add(new AsignacionError(CatalogoErrores.SinStock,
                        $"{producto.Nombre} ({producto.Sku}): se piden {cantidad} y no hay unidades en ninguna ubicación.",
                        productoId, producto.Sku.Valor, cantidad, 0));
                }
                else if (disponible < cantidad)
                {
                    var detalle = string.Join(", ", existencias.Where(e => e.Cantidad > 0).Select(e => $"{ubicaciones[e.UbicacionId].Nombre}: {e.Cantidad}"));
                    errores.Add(new AsignacionError(CatalogoErrores.StockInsuficiente,
                        $"{producto.Nombre} ({producto.Sku}): se piden {cantidad} y solo hay {disponible} ({detalle}).",
                        productoId, producto.Sku.Valor, cantidad, disponible));
                }
            }

            if (errores.Count > 0)
            {
                return AsignacionOrden.Rechazar(ordenCompraId, errores);
            }

            // 3. Todo alcanza: se descuenta recogiendo primero de las ubicaciones con menor prioridad.
            var detalles = new List<AsignacionDetalle>();
            foreach (var (productoId, (producto, cantidad)) in solicitado)
            {
                var pendiente = cantidad;
                foreach (var existencia in Ordenar(existenciasPorProducto[productoId], ubicaciones))
                {
                    if (pendiente == 0)
                    {
                        break;
                    }

                    var tomar = Math.Min(pendiente, existencia.Cantidad);
                    existencia.Descontar(Cantidad.Crear(tomar));
                    detalles.Add(new AsignacionDetalle(producto, ubicaciones[existencia.UbicacionId], tomar));
                    pendiente -= tomar;
                }
            }

            return AsignacionOrden.Asignar(ordenCompraId, detalles);
        }

        /// <summary>
        /// Orden de recogida: prioridad de la ubicación (menor primero), luego donde haya más unidades, para partir el pedido lo menos posible.
        /// </summary>
        private static List<Existencia> Ordenar(IEnumerable<Existencia> existencias, IReadOnlyDictionary<Guid, Ubicacion> ubicaciones)
        {
            return existencias
                .Where(e => e.Cantidad > 0 && ubicaciones.ContainsKey(e.UbicacionId))
                .OrderBy(e => ubicaciones[e.UbicacionId].Prioridad)
                .ThenByDescending(e => e.Cantidad)
                .ThenBy(e => ubicaciones[e.UbicacionId].Codigo)
                .ToList();
        }

        private static string Describir(Producto? producto, Guid? productoId, string? sku)
        {
            if (producto is not null)
            {
                return $"{producto.Nombre} ({producto.Sku})";
            }
            return sku is not null ? $"con SKU {sku}" : $"{productoId}";
        }
    }
}
