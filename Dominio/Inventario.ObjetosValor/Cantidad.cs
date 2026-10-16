using Inventario.Dominio.Comun;

namespace Inventario.ObjetosValor
{
    /// <summary>
    /// Cantidad de unidades que se mueve en una operación de inventario; siempre positiva.
    /// </summary>
    public sealed class Cantidad
    {
        public int Valor { get; private set; }

        private Cantidad(int valor)
        {
            if (valor <= 0)
            {
                throw new ExcepcionValidacion(CatalogoErrores.CantidadInvalida, $"La cantidad debe ser mayor a 0 (recibido: {valor}).");
            }
            Valor = valor;
        }

        public static Cantidad Crear(int valor)
        {
            return new Cantidad(valor);
        }

        public override bool Equals(object? obj) => obj is Cantidad otra && otra.Valor == Valor;

        public override int GetHashCode() => Valor.GetHashCode();
    }
}
