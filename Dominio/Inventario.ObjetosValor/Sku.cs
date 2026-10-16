using Inventario.Dominio.Comun;

namespace Inventario.ObjetosValor
{
    /// <summary>
    /// Código de producto (Stock Keeping Unit). Se guarda en mayúsculas y sin espacios en los extremos.
    /// </summary>
    public sealed class Sku
    {
        public const int LongitudMaxima = 50;

        public string Valor { get; private set; }

        private Sku(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ExcepcionValidacion(CatalogoErrores.SkuInvalido, "El SKU es obligatorio.");
            }

            valor = valor.Trim().ToUpperInvariant();
            if (valor.Length > LongitudMaxima)
            {
                throw new ExcepcionValidacion(CatalogoErrores.SkuInvalido, $"El SKU no puede superar {LongitudMaxima} caracteres.");
            }
            Valor = valor;
        }

        public static Sku Crear(string valor)
        {
            return new Sku(valor);
        }

        public override bool Equals(object? obj) => obj is Sku otro && otro.Valor == Valor;

        public override int GetHashCode() => Valor.GetHashCode();

        public override string ToString() => Valor;
    }
}
