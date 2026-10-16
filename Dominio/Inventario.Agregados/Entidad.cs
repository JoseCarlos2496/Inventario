namespace Inventario.Agregados
{
    public abstract class Entidad
    {
        public Guid Id { get; private set; }

        public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; private set; } = DateTime.UtcNow;

        protected Entidad()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Para entidades cuya identidad viene de otro contexto (p. ej. réplicas por uuid).
        /// </summary>
        protected Entidad(Guid id)
        {
            Id = id;
        }

        protected void MarcarModificado()
        {
            FechaModificacion = DateTime.UtcNow;
        }
    }
}
