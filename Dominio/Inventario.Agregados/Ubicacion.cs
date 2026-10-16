using Inventario.Dominio.Comun;
using Inventario.ObjetosValor;

namespace Inventario.Agregados
{
    /// <summary>
    /// Lugar físico donde se guarda stock (bodega, tienda, ...). Al surtir una orden se recoge primero
    /// de las ubicaciones con menor <see cref="Prioridad"/>.
    /// </summary>
    public class Ubicacion : AgregadoRoot
    {
        public const int LongitudMaximaCodigo = 30;

        public string Codigo { get; private set; } = null!;

        public Nombre Nombre { get; private set; } = null!;

        public TipoUbicacion Tipo { get; private set; }

        public int Prioridad { get; private set; }

        private Ubicacion() { }

        private Ubicacion(string codigo, Nombre nombre, TipoUbicacion tipo, int prioridad)
        {
            Codigo = NormalizarCodigo(codigo);
            Nombre = nombre;
            Tipo = tipo;
            DefinirPrioridad(prioridad);
        }

        public static Ubicacion Crear(string codigo, Nombre nombre, TipoUbicacion tipo, int prioridad)
        {
            return new Ubicacion(codigo, nombre, tipo, prioridad);
        }

        public static string NormalizarCodigo(string? codigo)
        {
            var valor = codigo?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(valor) || valor.Length > LongitudMaximaCodigo)
            {
                throw new ExcepcionValidacion(CatalogoErrores.CaracteristicaInvalida,
                    $"El código de ubicación es obligatorio y no puede superar {LongitudMaximaCodigo} caracteres.");
            }
            return valor;
        }

        public void DefinirPrioridad(int prioridad)
        {
            if (prioridad < 0)
            {
                throw new ExcepcionValidacion(CatalogoErrores.PrioridadInvalida);
            }
            Prioridad = prioridad;
            MarcarModificado();
        }
    }
}
