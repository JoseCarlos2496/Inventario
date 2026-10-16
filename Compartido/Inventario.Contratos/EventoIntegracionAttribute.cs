namespace Inventario.Contratos
{
    /// <summary>
    /// Nombre estable y versión con los que viaja un evento. El nombre de la clase C# puede cambiar sin romper consumidores.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EventoIntegracionAttribute : Attribute
    {
        public EventoIntegracionAttribute(string nombre, int version)
        {
            Nombre = nombre;
            Version = version;
        }

        public string Nombre { get; }

        public int Version { get; }

        public string NombreVersionado => $"{Nombre}.v{Version}";
    }
}
