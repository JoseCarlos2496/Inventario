using Inventario.Agregados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infraestructura.Configuracion
{
    public sealed class ExistenciaConfiguracion : IEntityTypeConfiguration<Existencia>
    {
        public void Configure(EntityTypeBuilder<Existencia> builder)
        {
            builder.ToTable("existencias");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Ignore(e => e.DomainEvents);

            builder.Property(e => e.ProductoId).IsRequired();
            builder.Property(e => e.UbicacionId).IsRequired();
            // Una fila por producto y ubicación.
            builder.HasIndex(e => new { e.ProductoId, e.UbicacionId }).IsUnique();

            builder.Property(e => e.Cantidad).IsRequired();
            builder.Property(e => e.Version).IsConcurrencyToken();
        }
    }
}
