using Inventario.Agregados;
using Inventario.Infraestructura.Configuracion;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Modelo
{
    public class ModeloInventario : DbContext
    {
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<Existencia> Existencias { get; set; }
        public DbSet<AsignacionOrden> Asignaciones { get; set; }
        public DbSet<MovimientoStock> Movimientos { get; set; }
        public DbSet<InventarioProducto> Inventarios { get; set; }
        public DbSet<MensajeOutbox> Outbox { get; set; }
        public DbSet<EventoRecibido> EventosRecibidos { get; set; }

        public ModeloInventario(DbContextOptions<ModeloInventario> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExistenciaConfiguracion).Assembly);

            modelBuilder.Entity<MensajeOutbox>(builder =>
            {
                builder.ToTable("outbox");
                builder.HasKey(m => m.Id);
                builder.Property(m => m.Id).ValueGeneratedNever();
                builder.Property(m => m.Tipo).HasMaxLength(150).IsRequired();
                builder.Property(m => m.Contenido).IsRequired();
                builder.Property(m => m.UltimoError).HasMaxLength(2000);
                builder.HasIndex(m => new { m.ProcesadoEn, m.FallidoEn, m.OcurridoEn });
            });

            modelBuilder.Entity<EventoRecibido>(builder =>
            {
                builder.ToTable("eventos_recibidos");
                builder.HasKey(e => e.Id);
                builder.Property(e => e.Id).ValueGeneratedNever();
                builder.Property(e => e.Tipo).HasMaxLength(150).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
