// <copyright file="ReaderService.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Implementations
{
    using FluentValidation;
    using FluentValidation.Results;
    using Library.Domain.Entities;
    using Library.Domain.Repositories;
    using Library.Services.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service for managing reader accoounts.
    /// </summary>
    /// <seealso cref="Library.Services.Interfaces.IReaderService" />
    public class ReaderService : IReaderService
    {
        private readonly IRepository<Reader> readerRepository;
        private readonly ILogger<ReaderService> logger;
        private readonly IValidator<Reader> validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderService"/> class.
        /// </summary>
        /// <param name="readerRepository">The reader repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="validator">The validator.</param>
        public ReaderService(
            IRepository<Reader> readerRepository,
            ILogger<ReaderService> logger,
            IValidator<Reader> validator)
        {
            this.readerRepository = readerRepository;
            this.logger = logger;
            this.validator = validator;
        }

        /// <summary>
        /// Registers the reader.
        /// </summary>
        /// <param name="reader">The reader entity.</param>
        /// <exception cref="ArgumentException">Represents a validation error.</exception>
        public void RegisterReader(Reader reader)
        {
            this.logger.LogInformation($"Registering new reader: {reader.FirstName} {reader.LastName}");
            ValidationResult validationResult = this.validator.Validate(reader);

            if (!validationResult.IsValid)
            {
                string errorMsg = string.Join("; ", validationResult.Errors);
                this.logger.LogWarning($"Validation failed for reader: {errorMsg}");
                throw new ArgumentException(errorMsg);
            }

            try
            {
                this.readerRepository.Add(reader);
                this.readerRepository.SaveChanges();
                this.logger.LogInformation($"Reader registered successfully. ID: {reader.Id}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error saving reader to database.");
                throw;
            }
        }

        /// <summary>
        /// Gets all readers.
        /// </summary>
        /// <returns>
        /// A list of all readers.
        /// </returns>
        public IEnumerable<Reader> GetAllReaders()
        {
            this.logger.LogInformation("Attempting to get all readers...");
            return this.readerRepository.GetAll();
        }

        /// <summary>
        /// Gets the reader by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The reader if it exists.
        /// </returns>
        public Reader? GetReaderById(Guid id)
        {
            this.logger.LogInformation("Attempting to get reader with ID: {id}", id);
            return this.readerRepository.GetById(id);
        }
    }
}
