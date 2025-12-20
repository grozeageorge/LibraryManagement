// <copyright file="BookService.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Implementations
{
    using FluentValidation;
    using FluentValidation.Results;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service for managing books in the library.
    /// </summary>
    /// <seealso cref="Library.Services.Interfaces.IBookService" />
    public class BookService : IBookService
    {
        private readonly IRepository<Book> bookRepository;
        private readonly ILogger<BookService> logger;
        private readonly IValidator<Book> validator;
        private readonly ILibraryConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookService"/> class.
        /// </summary>
        /// <param name="bookRepository">The book repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="configuration">The configuration.</param>
        public BookService(
            IRepository<Book> bookRepository,
            ILogger<BookService> logger,
            IValidator<Book> validator,
            ILibraryConfiguration configuration)
        {
            this.bookRepository = bookRepository;
            this.logger = logger;
            this.validator = validator;
            this.configuration = configuration;
        }

        /// <summary>
        /// Adds a new book after validating it.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <exception cref="ArgumentException">Represents a validation error.</exception>
        /// <exception cref="InvalidOperationException">Represents a configuration constraint error.</exception>
        public void AddBook(Book book)
        {
            this.logger.LogInformation($"Attempting to add a new book: {book.Title}");

            ValidationResult validationResult = this.validator.Validate(book);
            if (!validationResult.IsValid)
            {
                string errorMsg = string.Join("; ", validationResult.Errors);
                this.logger.LogWarning($"Validation failed for book '{book.Title}': {errorMsg}");
                throw new ArgumentException(errorMsg);
            }

            if (book.Domains.Count > this.configuration.MaxDomainsPerBook)
            {
                string errorMsg = $"Book exceeds maximum allowed domains ({this.configuration.MaxDomainsPerBook}).";
                this.logger.LogWarning(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            try
            {
                this.bookRepository.Add(book);
                this.bookRepository.SaveChanges();
                this.logger.LogInformation($"Book '{book.Title}' added successfully with ID {book.Id}.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occurred while adding book '{book.Title}'.");
                throw;
            }
        }

        /// <summary>
        /// Gets all books.
        /// </summary>
        /// <returns>
        /// A list of books.
        /// </returns>
        public IEnumerable<Book> GetAllBooks()
        {
            this.logger.LogInformation("Retrieving all books from the repository.");
            return this.bookRepository.GetAll();
        }

        /// <summary>
        /// Gets the book by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The book entity.
        /// </returns>
        public Book? GetBookById(Guid id)
        {
            this.logger.LogInformation($"Retrieving book with ID {id}.");
            return this.bookRepository.GetById(id);
        }
    }
}
