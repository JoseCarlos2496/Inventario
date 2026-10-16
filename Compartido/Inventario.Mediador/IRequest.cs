namespace Inventario.Mediador
{
    public interface IRequest<TResponse> { }

    /// <summary>
    /// Respuesta vacía para comandos que no devuelven nada.
    /// </summary>
    public readonly record struct Unidad
    {
        public static readonly Unidad Valor = new();
    }
}
