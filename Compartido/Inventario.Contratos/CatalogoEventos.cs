using System.Reflection;
using System.Text.Json;

namespace Inventario.Contratos
{
    /// <summary>
    /// Resuelve nombre estable + versión con el tipo C#, y envuelve/desenvuelve eventos en un <see cref="SobreEvento"/>.
    /// </summary>
    public static class CatalogoEventos
    {
        public static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        private static readonly Dictionary<Type, EventoIntegracionAttribute> _porTipo = typeof(CatalogoEventos).Assembly
            .GetTypes()
            .Where(tipo => !tipo.IsAbstract && typeof(IEventoIntegracion).IsAssignableFrom(tipo))
            .Select(tipo => (Tipo: tipo, Atributo: tipo.GetCustomAttribute<EventoIntegracionAttribute>()))
            .Where(x => x.Atributo is not null)
            .ToDictionary(x => x.Tipo, x => x.Atributo!);

        private static readonly Dictionary<string, Type> _porNombre = _porTipo
            .ToDictionary(x => x.Value.NombreVersionado, x => x.Key);

        public static IReadOnlyDictionary<Type, EventoIntegracionAttribute> Todos => _porTipo;

        public static EventoIntegracionAttribute Describir(Type tipo)
        {
            return _porTipo.TryGetValue(tipo, out var atributo)
                ? atributo
                : throw new InvalidOperationException($"El tipo {tipo.Name} no tiene [EventoIntegracion].");
        }

        public static Type? Resolver(string nombre, int version)
        {
            return _porNombre.GetValueOrDefault($"{nombre}.v{version}");
        }

        public static SobreEvento Envolver(IEventoIntegracion evento, string origen)
        {
            var atributo = Describir(evento.GetType());
            var payload = JsonSerializer.SerializeToElement(evento, evento.GetType(), OpcionesJson);
            return new SobreEvento(Guid.NewGuid(), atributo.Nombre, atributo.Version, DateTime.UtcNow, origen, payload);
        }

        /// <summary>
        /// Devuelve el evento tipado, o null si el nombre/versión no está en el catálogo.
        /// </summary>
        public static IEventoIntegracion? Desenvolver(SobreEvento sobre)
        {
            var tipo = Resolver(sobre.Tipo, sobre.Version);
            if (tipo is null)
            {
                return null;
            }

            return (IEventoIntegracion?)sobre.Payload.Deserialize(tipo, OpcionesJson);
        }
    }
}
