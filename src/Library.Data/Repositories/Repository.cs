// <copyright file="Repository.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Data.Repositories
{
    using System.Linq.Expressions;
    using Library.Domain.Entities;
    using Library.Domain.Repositories;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Generic implementation of the repository pattern using Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <seealso cref="Library.Domain.Repositories.IRepository&lt;T&gt;" />
    public class Repository<T> : IRepository<T>
        where T : BaseEntity
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private readonly LibraryDbContext context;

        /// <summary>
        /// The database set for the entity.
        /// </summary>
        private readonly DbSet<T> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public Repository(LibraryDbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        /// <summary>
        /// Adds the specified entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void Add(T entity)
        {
            this.dbSet.Add(entity);
        }

        /// <summary>
        /// Finds entities matching a specific condition.
        /// </summary>
        /// <param name="predicate">The condition expression.</param>
        /// <returns>
        /// A collection of matching entities.
        /// </returns>
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return this.dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>
        /// A collection of all entities.
        /// </returns>
        public IEnumerable<T> GetAll()
        {
            return this.dbSet.ToList();
        }

        /// <summary>
        /// Gets an entity the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The entity if found; otherwise null.
        /// </returns>
        public T? GetById(Guid id)
        {
            return this.dbSet.Find(id);
        }

        /// <summary>
        /// Removes the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void Remove(T entity)
        {
            this.dbSet.Remove(entity);
        }

        /// <summary>
        /// Saves the changes made in this context to the database.
        /// </summary>
        public void SaveChanges()
        {
            this.context.SaveChanges();
        }
    }
}
