using System.Text.Json;
using Inventario.Agregados;
using Inventario.ObjetosValor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Infraestructura.Configuracion
{
    public sealed class ProductoConfiguracion : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("productos");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Ignore(p => p.DomainEvents);

            builder.Property(p => p.Sku)
                .HasConversion(sku => sku.Valor, valor => Sku.Crear(valor))
                .HasColumnName("sku")
                .HasMaxLength(Sku.LongitudMaxima)
                .IsRequired();
            builder.HasIndex(p => p.Sku).IsUnique();

            builder.Property(p => p.Nombre)
                .HasConversion(nombre => nombre.Valor, valor => Nombre.Crear(valor))
                .HasColumnName("nombre")
                .HasMaxLength(Nombre.LongitudMaxima)
                .IsRequired();

            builder.Property(p => p.Descripcion).HasMaxLength(Producto.LongitudMaximaDescripcion);
            builder.Property(p => p.Marca).HasMaxLength(Producto.LongitudMaximaTexto);
            builder.Property(p => p.Modelo).HasMaxLength(Producto.LongitudMaximaTexto);
            builder.Property(p => p.Categoria).HasMaxLength(Producto.LongitudMaximaTexto);
            builder.Property(p => p.UnidadMedida).HasMaxLength(Producto.LongitudMaximaTexto);

            // Características libres: una columna de texto con JSON, igual en SqlServer, PostgreSQL e InMemory.
            builder.Ignore(p => p.Caracteristicas);
            builder.Property<Dictionary<string, string>>("_caracteristicas")
                .HasColumnName("caracteristicas")
                .HasConversion(
                    valor => JsonSerializer.Serialize(valor, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(),
                    new ValueComparer<Dictionary<string, string>>(
                        (a, b) => a!.Count == b!.Count && !a.Except(b).Any(),
                        d => d.Aggregate(0, (hash, par) => HashCode.Combine(hash, par.Key, par.Value)),
                        d => new Dictionary<string, string>(d)))
                .IsRequired();
        }
    }
}
