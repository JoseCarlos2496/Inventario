using Inventario.Agregados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infraestructura.Configuracion
{
    public sealed class AsignacionOrdenConfiguracion : IEntityTypeConfiguration<AsignacionOrden>
    {
        public void Configure(EntityTypeBuilder<AsignacionOrden> builder)
        {
            builder.ToTable("asignaciones");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedNever();
            builder.Ignore(a => a.DomainEvents);

            builder.Property(a => a.OrdenCompraId).IsRequired();
            builder.HasIndex(a => a.OrdenCompraId).IsUnique();

            builder.Property(a => a.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(a => a.Motivo).HasMaxLength(AsignacionError.LongitudMaximaMensaje);

            // Las colecciones públicas son de solo lectura; EF trabaja sobre los campos privados.
            builder.HasMany(a => a.Detalles)
                .WithOne()
                .HasForeignKey(d => d.AsignacionOrdenId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.Detalles).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(a => a.Errores)
                .WithOne()
                .HasForeignKey(e => e.AsignacionOrdenId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.Errores).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public sealed class AsignacionDetalleConfiguracion : IEntityTypeConfiguration<AsignacionDetalle>
    {
        public void Configure(EntityTypeBuilder<AsignacionDetalle> builder)
        {
            builder.ToTable("asignaciones_detalle");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).ValueGeneratedNever();
            builder.Property(d => d.ProductoId).IsRequired();
            builder.Property(d => d.Sku).HasMaxLength(50).IsRequired();
            builder.Property(d => d.NombreProducto).HasMaxLength(200).IsRequired();
            builder.Property(d => d.UbicacionId).IsRequired();
            builder.Property(d => d.CodigoUbicacion).HasMaxLength(Ubicacion.LongitudMaximaCodigo).IsRequired();
            builder.Property(d => d.NombreUbicacion).HasMaxLength(200).IsRequired();
            builder.Property(d => d.TipoUbicacion).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(d => d.Cantidad).IsRequired();
        }
    }

    public sealed class AsignacionErrorConfiguracion : IEntityTypeConfiguration<AsignacionError>
    {
        public void Configure(EntityTypeBuilder<AsignacionError> builder)
        {
            builder.ToTable("asignaciones_error");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Mensaje).HasMaxLength(AsignacionError.LongitudMaximaMensaje).IsRequired();
            builder.Property(e => e.Sku).HasMaxLength(50);
        }
    }
}
