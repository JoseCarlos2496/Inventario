using Inventario.Dominio.Comun;
using Inventario.ObjetosValor;

namespace Inventario.Agregados
{
    /// <summary>
    /// Ficha del producto. Lo debería dar de alta Compras; mientras no exista, lo crea la simulación de compra.
    /// </summary>
    public class Producto : AgregadoRoot
    {
        public const int LongitudMaximaTexto = 100;
        public const int LongitudMaximaDescripcion = 1000;

        private Dictionary<string, string> _caracteristicas = [];

        public Sku Sku { get; private set; } = null!;

        public Nombre Nombre { get; private set; } = null!;

        public string? Descripcion { get; private set; }

        public string? Marca { get; private set; }

        public string? Modelo { get; private set; }

        public string? Categoria { get; private set; }

        public string? UnidadMedida { get; private set; }

        /// <summary>Atributos libres del producto (p. ej. "RAM": "16 GB", "Color": "Gris").</summary>
        public IReadOnlyDictionary<string, string> Caracteristicas => _caracteristicas;

        private Producto() { }

        private Producto(Sku sku, Nombre nombre)
        {
            Sku = sku;
            Nombre = nombre;
        }

        public static Producto Crear(Sku sku, Nombre nombre, DatosProducto datos)
        {
            var producto = new Producto(sku, nombre);
            producto.Actualizar(nombre, datos);
            return producto;
        }

        /// <summary>
        /// Los datos en null se conservan; las características recibidas se agregan o reemplazan por clave.
        /// </summary>
        public void Actualizar(Nombre nombre, DatosProducto datos)
        {
            Nombre = nombre;
            Descripcion = Texto(datos.Descripcion, LongitudMaximaDescripcion, "descripción") ?? Descripcion;
            Marca = Texto(datos.Marca, LongitudMaximaTexto, "marca") ?? Marca;
            Modelo = Texto(datos.Modelo, LongitudMaximaTexto, "modelo") ?? Modelo;
            Categoria = Texto(datos.Categoria, LongitudMaximaTexto, "categoría") ?? Categoria;
            UnidadMedida = Texto(datos.UnidadMedida, LongitudMaximaTexto, "unidad de medida") ?? UnidadMedida;

            if (datos.Caracteristicas is not null)
            {
                var caracteristicas = new Dictionary<string, string>(_caracteristicas);
                foreach (var (clave, valor) in datos.Caracteristicas)
                {
                    var claveLimpia = Texto(clave, LongitudMaximaTexto, "clave de característica")
                        ?? throw new ExcepcionValidacion(CatalogoErrores.CaracteristicaInvalida, "Las características no pueden tener clave vacía.");
                    caracteristicas[claveLimpia] = Texto(valor, LongitudMaximaTexto, $"característica {claveLimpia}") ?? string.Empty;
                }
                // Se reemplaza la instancia para que EF detecte el cambio de la columna JSON.
                _caracteristicas = caracteristicas;
            }

            MarcarModificado();
        }

        private static string? Texto(string? valor, int longitudMaxima, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            valor = valor.Trim();
            if (valor.Length > longitudMaxima)
            {
                throw new ExcepcionValidacion(CatalogoErrores.CaracteristicaInvalida, $"El campo {campo} no puede superar {longitudMaxima} caracteres.");
            }
            return valor;
        }
    }

    public sealed record DatosProducto(
        string? Descripcion,
        string? Marca,
        string? Modelo,
        string? Categoria,
        string? UnidadMedida,
        IReadOnlyDictionary<string, string>? Caracteristicas);
}
