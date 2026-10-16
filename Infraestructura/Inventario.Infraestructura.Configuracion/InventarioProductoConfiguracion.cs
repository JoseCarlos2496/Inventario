using Inventario.Agregados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infraestructura.Configuracion
{
    public sealed class InventarioProductoConfiguracion : IEntityTypeConfiguration<InventarioProducto>
    {
        public void Configure(EntityTypeBuilder<InventarioProducto> builder)
        {
            builder.ToTable("inventarios");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedNever();
            builder.Ignore(i => i.DomainEvents);

            builder.Property(i => i.ProductoId).IsRequired();
            builder.HasIndex(i => i.ProductoId).IsUnique();
            builder.Property(i => i.StockMinimo).IsRequired();
            builder.Property(i => i.BajoMinimo).IsRequired();
        }
    }
}
