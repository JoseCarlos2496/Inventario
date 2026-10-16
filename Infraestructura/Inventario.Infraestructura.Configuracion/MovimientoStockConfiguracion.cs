using Inventario.Agregados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infraestructura.Configuracion
{
    public sealed class MovimientoStockConfiguracion : IEntityTypeConfiguration<MovimientoStock>
    {
        public void Configure(EntityTypeBuilder<MovimientoStock> builder)
        {
            builder.ToTable("movimientos_stock");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedNever();
            builder.Ignore(m => m.DomainEvents);

            builder.Property(m => m.UbicacionId).IsRequired();
            builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(m => m.Cantidad).IsRequired();
            builder.Property(m => m.Referencia).IsRequired();
            builder.HasIndex(m => new { m.ProductoId, m.Fecha });
        }
    }
}
