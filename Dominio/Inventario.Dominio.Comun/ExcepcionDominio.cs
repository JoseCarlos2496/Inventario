namespace Inventario.Dominio.Comun
{
    /// <summary>
    /// Regla de negocio incumplida. El <see cref="Codigo"/> (p. ej. EXI-002, ver <see cref="CatalogoErrores"/>) viaja tal cual en la respuesta HTTP.
    /// </summary>
    public abstract class ExcepcionDominio : Exception
    {
        protected ExcepcionDominio(string codigo, string mensaje) : base(mensaje)
        {
            Codigo = codigo;
        }

        public string Codigo { get; }
    }

    /// <summary>Datos de entrada inválidos (HTTP 400).</summary>
    public sealed class ExcepcionValidacion(string codigo, string mensaje) : ExcepcionDominio(codigo, mensaje)
    {
        public ExcepcionValidacion(DefinicionError error, string? mensaje = null) : this(error.Codigo, mensaje ?? error.Descripcion) { }
    }

    /// <summary>El recurso no existe (HTTP 404).</summary>
    public sealed class ExcepcionNoEncontrado(string codigo, string mensaje) : ExcepcionDominio(codigo, mensaje)
    {
        public ExcepcionNoEncontrado(DefinicionError error, string? mensaje = null) : this(error.Codigo, mensaje ?? error.Descripcion) { }
    }

    /// <summary>El estado actual no permite la operación (HTTP 409).</summary>
    public sealed class ExcepcionConflicto(string codigo, string mensaje) : ExcepcionDominio(codigo, mensaje)
    {
        public ExcepcionConflicto(DefinicionError error, string? mensaje = null) : this(error.Codigo, mensaje ?? error.Descripcion) { }
    }
}
