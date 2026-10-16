using Inventario.Dominio.Comun;

namespace Inventario.ObjetosValor
{
    public sealed class Nombre
    {
        public const int LongitudMaxima = 200;

        public string Valor { get; private set; }

        private Nombre(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ExcepcionValidacion(CatalogoErrores.NombreInvalido, "El nombre es obligatorio.");
            }

            valor = valor.Trim();
            if (valor.Length > LongitudMaxima)
            {
                throw new ExcepcionValidacion(CatalogoErrores.NombreInvalido, $"El nombre no puede superar {LongitudMaxima} caracteres.");
            }
            Valor = valor;
        }

        public static Nombre Crear(string valor)
        {
            return new Nombre(valor);
        }

        public override bool Equals(object? obj) => obj is Nombre otro && otro.Valor == Valor;

        public override int GetHashCode() => Valor.GetHashCode();

        public override string ToString() => Valor;
    }
}
