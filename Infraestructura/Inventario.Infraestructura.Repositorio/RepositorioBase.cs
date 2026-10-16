using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Repositorio
{
    public abstract class RepositorioBase<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        protected RepositorioBase(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public void Guardar(T entidad)
        {
            _dbSet.Add(entidad);
        }
    }
}
