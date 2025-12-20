// <copyright file="IRepository.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Repositories
{
    using System.Linq.Expressions;
    using Library.Domain.Entities;

    /// <summary>
    /// Generic repository interface defining standard CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type derived from BaseEntity.</typeparam>
    public interface IRepository<T>
        where T : BaseEntity
    {
        /// <summary>
        /// Gets an entity the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The entity if found; otherwise null.</returns>
        T? GetById(Guid id);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Finds entities matching a specific condition.
        /// </summary>
        /// <param name="predicate">The condition expression.</param>
        /// <returns>A collection of matching entities.</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds the specified entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(T entity);

        /// <summary>
        /// Removes the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void Remove(T entity);

        /// <summary>
        /// Saves the changes made in this context to the database.
        /// </summary>
        void SaveChanges();
    }
}
