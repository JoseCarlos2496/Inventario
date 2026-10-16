using Inventario.Agregados;
using Inventario.ObjetosValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infraestructura.Configuracion
{
    public sealed class UbicacionConfiguracion : IEntityTypeConfiguration<Ubicacion>
    {
        public void Configure(EntityTypeBuilder<Ubicacion> builder)
        {
            builder.ToTable("ubicaciones");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedNever();
            builder.Ignore(u => u.DomainEvents);

            builder.Property(u => u.Codigo).HasMaxLength(Ubicacion.LongitudMaximaCodigo).IsRequired();
            builder.HasIndex(u => u.Codigo).IsUnique();

            builder.Property(u => u.Nombre)
                .HasConversion(nombre => nombre.Valor, valor => Nombre.Crear(valor))
                .HasColumnName("nombre")
                .HasMaxLength(Nombre.LongitudMaxima)
                .IsRequired();

            builder.Property(u => u.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(u => u.Prioridad).IsRequired();
        }
    }
}
