using System.Reflection;

namespace Inventario.Dominio.Comun
{
    /// <summary>
    /// Cómo se comunica el error: los cuatro primeros viajan como respuesta HTTP; <see cref="Rechazo"/> viaja
    /// dentro del evento <c>inventario.orden.rechazada</c> (la orden se procesó, pero no se pudo surtir).
    /// </summary>
    public enum TipoError
    {
        Validacion,
        NoEncontrado,
        Conflicto,
        Sistema,
        Rechazo
    }

    public sealed record DefinicionError(string Codigo, TipoError Tipo, string Descripcion);

    /// <summary>
    /// Catálogo único de códigos de error. Todo código que lance el dominio o publique un evento sale de aquí;
    /// <c>GET /api/inventario/errores/catalogo</c> lo expone a los demás equipos.
    /// </summary>
    public static class CatalogoErrores
    {
        // Validación de datos de entrada
        public static readonly DefinicionError FormatoInvalido = new("VAL-000", TipoError.Validacion, "El body no tiene un formato válido (ver errors[]).");
        public static readonly DefinicionError CantidadInvalida = new("VAL-001", TipoError.Validacion, "La cantidad debe ser mayor a 0.");
        public static readonly DefinicionError SkuInvalido = new("VAL-002", TipoError.Validacion, "SKU vacío o de más de 50 caracteres.");
        public static readonly DefinicionError NombreInvalido = new("VAL-003", TipoError.Validacion, "Nombre vacío o de más de 200 caracteres.");
        public static readonly DefinicionError CaracteristicaInvalida = new("VAL-004", TipoError.Validacion, "Dato o característica del producto inválido (clave vacía o texto demasiado largo).");

        // Productos
        public static readonly DefinicionError ProductoNoEncontrado = new("PRO-001", TipoError.NoEncontrado, "El producto no existe.");

        // Ubicaciones
        public static readonly DefinicionError UbicacionNoEncontrada = new("UBI-001", TipoError.NoEncontrado, "La ubicación no existe.");
        public static readonly DefinicionError UbicacionDuplicada = new("UBI-002", TipoError.Conflicto, "Ya existe una ubicación con ese código.");
        public static readonly DefinicionError PrioridadInvalida = new("UBI-003", TipoError.Validacion, "La prioridad de la ubicación no puede ser negativa.");

        // Existencias
        public static readonly DefinicionError ExistenciaSinDatos = new("EXI-001", TipoError.Validacion, "La existencia requiere producto y ubicación.");
        public static readonly DefinicionError StockInsuficienteUbicacion = new("EXI-002", TipoError.Conflicto, "La ubicación no tiene unidades suficientes para descontar.");

        // Compras (simulación)
        public static readonly DefinicionError CompraSinLineas = new("COM-001", TipoError.Validacion, "La compra no tiene líneas.");
        public static readonly DefinicionError CompraDuplicada = new("COM-002", TipoError.Conflicto, "Esa compra (compraId) ya fue registrada; no se vuelve a ingresar stock.");

        // Salud del inventario
        public static readonly DefinicionError StockMinimoInvalido = new("INV-001", TipoError.Validacion, "El stock mínimo no puede ser negativo.");

        // Órdenes de compra: errores de la petición
        public static readonly DefinicionError OrdenSinIdentificador = new("ORD-001", TipoError.Validacion, "La orden de compra no trae ordenCompraId.");
        public static readonly DefinicionError OrdenNoProcesada = new("ORD-002", TipoError.NoEncontrado, "Inventario no ha procesado esa orden de compra.");

        // Órdenes de compra: motivos de rechazo (viajan en inventario.orden.rechazada, no como HTTP de error)
        public static readonly DefinicionError OrdenSinLineas = new("ORD-100", TipoError.Rechazo, "La orden de compra no tiene líneas.");
        public static readonly DefinicionError ProductoNoRegistrado = new("ORD-101", TipoError.Rechazo, "El producto solicitado no está registrado en Inventario.");
        public static readonly DefinicionError StockInsuficiente = new("ORD-102", TipoError.Rechazo, "La suma de todas las ubicaciones no alcanza la cantidad solicitada.");
        public static readonly DefinicionError SinStock = new("ORD-103", TipoError.Rechazo, "El producto no tiene unidades en ninguna ubicación.");
        public static readonly DefinicionError CantidadSolicitadaInvalida = new("ORD-104", TipoError.Rechazo, "La línea pide una cantidad menor o igual a 0.");
        public static readonly DefinicionError LineaSinProducto = new("ORD-105", TipoError.Rechazo, "La línea no indica productoId ni sku.");

        // Eventos y sistema
        public static readonly DefinicionError EventoNoConsumido = new("EVT-001", TipoError.Validacion, "Inventario no consume ese evento (tipo/versión).");
        public static readonly DefinicionError EventoInvalido = new("EVT-002", TipoError.Validacion, "El payload del evento no es válido.");
        public static readonly DefinicionError EventoOutboxNoEncontrado = new("SYS-001", TipoError.NoEncontrado, "El evento de la Outbox no existe.");
        public static readonly DefinicionError EventoNoFallido = new("SYS-002", TipoError.Conflicto, "El evento no está fallido.");
        public static readonly DefinicionError ConflictoConcurrencia = new("GEN-409", TipoError.Conflicto, "El stock cambió mientras se procesaba la operación; reintentar.");
        public static readonly DefinicionError ErrorInesperado = new("GEN-500", TipoError.Sistema, "Error inesperado.");

        private static readonly IReadOnlyList<DefinicionError> _todos = typeof(CatalogoErrores)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(DefinicionError))
            .Select(f => (DefinicionError)f.GetValue(null)!)
            .OrderBy(e => e.Codigo)
            .ToList();

        public static IReadOnlyList<DefinicionError> Todos => _todos;
    }
}
