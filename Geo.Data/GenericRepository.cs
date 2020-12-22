using Geo.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Geo.Data
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal DbSet<TEntity> dbSet;
        public GenericRepository(GeoDbContext context)
        {
            dbSet = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            string filterAttribute = null)
        {
            IQueryable<TEntity> query = dbSet;

            if (filterAttribute != null)
            {
                query = query.IgnoreQueryFilters();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query);
            }
            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public IQueryable<TEntity> GetById(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> result = dbSet.Where(filter);

            if (include != null)
                result = include(result);

            return result.AsQueryable();
        }

        public virtual void Create(TEntity entity) => dbSet.Add(entity);

    }
}
