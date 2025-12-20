// <copyright file="IBookService.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Interfaces
{
    using Library.Domain.Entities;

    /// <summary>
    /// Interface for managing books in the library.
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Adds a new book after validating it.
        /// </summary>
        /// <param name="book">The book.</param>
        void AddBook(Book book);

        /// <summary>
        /// Gets the book by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The book entity.</returns>
        Book? GetBookById(Guid id);

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>A list of books.</returns>
        IEnumerable<Book> GetAllBooks();
    }
}
