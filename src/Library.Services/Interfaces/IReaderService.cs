// <copyright file="IReaderService.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Interfaces
{
    using Library.Domain.Entities;

    /// <summary>
    /// Interface for managing readers in the library.
    /// </summary>
    public interface IReaderService
    {
        /// <summary>
        /// Registers the reader.
        /// </summary>
        /// <param name="reader">The reader entity.</param>
        void RegisterReader(Reader reader);

        /// <summary>
        /// Gets the reader by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The reader if it exists.</returns>
        Reader? GetReaderById(Guid id);

        /// <summary>
        /// Gets all readers.
        /// </summary>
        /// <returns>A list of all readers.</returns>
        IEnumerable<Reader> GetAllReaders();
    }
}
