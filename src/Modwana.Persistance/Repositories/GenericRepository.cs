﻿using Modwana.Core.Entities;
using Modwana.Core.Interfaces;
using Modwana.Core.Search;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Modwana.Persistance.Repositories
{
    public class GenericRepository : IGenericRepository
    {
        internal ModwanaDbContext _context;

        public GenericRepository() { }

        public GenericRepository(ModwanaDbContext context)
        {
            _context = context;

        }

        public virtual async Task<TEntity> CreateAsync<TEntity>(TEntity entity) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            if (entity is AuditableEntity auditable)
                auditable.InsertAudit();

            await dbSet.AddAsync(entity);

            if (_context == null)
            {
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }

            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync<TEntity>(TEntity entityToUpdate) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            if (entityToUpdate is AuditableEntity auditableEntity)
                auditableEntity.UpdateAudit();

            context.Update(entityToUpdate);

            if (_context == null)
            {
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }

            return entityToUpdate;
        }

        public virtual async Task DeleteAsync<TEntity>(string id) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            var found = await dbSet.FindAsync(id);

            dbSet.Remove(found);

            if (_context == null)
            {
                await context.SaveChangesAsync();
                await context.DisposeAsync();
            }
        }

        public virtual async Task<int> CountAsync<TEntity>() where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            int count;

            count = await dbSet.CountAsync();

            if (_context == null)
                await context.DisposeAsync();

            return count;
        }

        public virtual async Task<int> CountAsync<TEntity>(SearchCriteria<TEntity> search) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            IQueryable<TEntity> query = dbSet;

            if (search.FilterExpression != null)
            {
                query = query.Where(search.FilterExpression);
            }

            int count = await query.CountAsync();

            if (_context == null)
                await context.DisposeAsync();

            return count;
        }

        public virtual async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            IQueryable<TEntity> query = dbSet;

            query = query.Where(filter);

            var count = await query.CountAsync();

            if (_context == null)
                await context.DisposeAsync();

            return count;
        }

        public virtual async Task<SearchResult<TEntity>> SearchAsync<TEntity>(SearchCriteria<TEntity> searchCriteria, params string[] includes) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            IQueryable<TEntity> query = dbSet;

            if (searchCriteria.FilterExpression != null)
            {
                query = query.Where(searchCriteria.FilterExpression);
            }

            if (includes != null && includes.Count() > 0)
            {
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (searchCriteria.SortExpression != null)
            {
                query = searchCriteria.SortExpression(query);
            }

            SearchResult<TEntity> result = new SearchResult<TEntity>(searchCriteria)
            {
                TotalResultsCount = await query.CountAsync(),
            };

            query = query.Skip(searchCriteria.StartIndex).Take(searchCriteria.PageSize);

            result.Result = await query.ToListAsync();

            if (_context == null)
                await context.DisposeAsync();

            return result;

        }

        public virtual async Task<TEntity> GetByIdAsync<TEntity>(string id, params string[] includes) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            IQueryable<TEntity> query = dbSet;

            if (includes != null && includes.Count() > 0)
            {
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
            }

            var entity = await query.Where(a => a.Id == id).FirstOrDefaultAsync();

            if (_context == null)
                await context.DisposeAsync();

            return entity;

        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync<TEntity>(
         Expression<Func<TEntity, bool>> filter = null,
         Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
         string[] includeProperties = null, int? maxSize = null) where TEntity : class, IBaseEntity
        {
            ModwanaDbContext context = _context ?? DbContextFactory.Create();

            var dbSet = context.Set<TEntity>();

            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (maxSize.HasValue)
                query = query.Take(maxSize.Value);


            var result = await query.ToListAsync();

            if (_context == null)
                await context.DisposeAsync();

            return result;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
