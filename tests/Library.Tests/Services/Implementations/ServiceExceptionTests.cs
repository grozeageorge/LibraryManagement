// <copyright file="ServiceExceptionTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Implementations
{
    using System;
    using FluentAssertions;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Tests.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Update;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Tests for the exceptions of the services and if they are logged correctly.
    /// </summary>
    [TestFixture]
    public class ServiceExceptionTests
    {
        // --- Mocks ---
        private Mock<IRepository<Book>> mockBookRepo;
        private Mock<IRepository<Reader>> mockReaderRepo;
        private Mock<IRepository<Loan>> mockLoanRepo;
        private Mock<IRepository<BookCopy>> mockCopyRepo;
        private Mock<ILogger<BookService>> mockBookLogger;
        private Mock<ILogger<LendingService>> mockLendingLogger;
        private Mock<FluentValidation.IValidator<Book>> mockBookValidator;
        private Mock<ILibraryConfiguration> mockConfig;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockBookRepo = new Mock<IRepository<Book>>();
            this.mockReaderRepo = new Mock<IRepository<Reader>>();
            this.mockLoanRepo = new Mock<IRepository<Loan>>();
            this.mockCopyRepo = new Mock<IRepository<BookCopy>>();
            this.mockBookLogger = new Mock<ILogger<BookService>>();
            this.mockLendingLogger = new Mock<ILogger<LendingService>>();
            this.mockBookValidator = new Mock<FluentValidation.IValidator<Book>>();
            this.mockConfig = new Mock<ILibraryConfiguration>();
        }

        /// <summary>
        /// Books service using add book method should log and throw when database fails.
        /// </summary>
        [Test]
        public void BookService_AddBook_ShouldLogAndThrow_WhenDatabaseFails()
        {
            // Arrange
            BookService service = new BookService(
                this.mockBookRepo.Object,
                this.mockBookLogger.Object,
                this.mockBookValidator.Object,
                this.mockConfig.Object);

            Book book = LibraryTestFactory.CreateBook();

            // Setup Validator to pass
            this.mockBookValidator.Setup(v => v.Validate(book)).Returns(new FluentValidation.Results.ValidationResult());
            this.mockConfig.SetupConfigDefaultLimits();

            // FORCE DATABASE ERROR
            this.mockBookRepo.Setup(r => r.SaveChanges()).Throws(new Exception("Database Connection Failed"));

            // Act
            Action act = () => service.AddBook(book);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Database Connection Failed");

            // Verify Logger was called (This covers the catch block!)
        }

        /// <summary>
        /// Lending service using borrow book method should log and throw when database fails.
        /// </summary>
        [Test]
        public void LendingService_BorrowBook_ShouldLogAndThrow_WhenDatabaseFails()
        {
            // Arrange
            LendingService service = new LendingService(
                this.mockLoanRepo.Object,
                this.mockReaderRepo.Object,
                this.mockCopyRepo.Object,
                this.mockBookRepo.Object,
                this.mockConfig.Object,
                this.mockLendingLogger.Object);

            this.mockConfig.SetupConfigDefaultLimits();

            Reader reader = LibraryTestFactory.CreateReader();
            BookCopy copy = LibraryTestFactory.CreateCopy();

            // Setup valid state
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(copy.Id, copy);
            this.mockCopyRepo.SetupFind(new[] { copy }); // Stock check

            // FORCE DATABASE ERROR on Save
            this.mockLoanRepo.Setup(r => r.SaveChanges()).Throws(new Exception("Critical DB Error"));

            // Act
            Action act = () => service.BorrowBook(reader.Id, copy.Id);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Critical DB Error");
        }

        /// <summary>
        /// Reader service using register reader method should log and throw when database fails.
        /// </summary>
        [Test]
        public void ReaderService_RegisterReader_ShouldLogAndThrow_WhenDatabaseFails()
        {
            // Arrange
            var service = new ReaderService(
                this.mockReaderRepo.Object,
                new Mock<ILogger<ReaderService>>().Object, // Create a specific logger mock for ReaderService
                new Mock<FluentValidation.IValidator<Reader>>().Object); // Create a dummy validator

            var reader = LibraryTestFactory.CreateReader();

            // Setup Validator to pass (using a loose mock for simplicity here)
            // Or if you want to use the field mock:
            // We need to instantiate the service with the class-level mocks if we want to control them.
            // Let's re-instantiate properly using the class fields to be safe.
            var mockReaderValidator = new Mock<FluentValidation.IValidator<Reader>>();
            mockReaderValidator.Setup(v => v.Validate(reader)).Returns(new FluentValidation.Results.ValidationResult());

            var readerService = new ReaderService(
                this.mockReaderRepo.Object,
                new Mock<ILogger<ReaderService>>().Object,
                mockReaderValidator.Object);

            // FORCE DATABASE ERROR
            this.mockReaderRepo.Setup(r => r.SaveChanges()).Throws(new Exception("Reader DB Error"));

            // Act
            Action act = () => readerService.RegisterReader(reader);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Reader DB Error");
        }

        /// <summary>
        /// Lending service using borrow book method should throw friendly error when concurrency conflict occurs.
        /// </summary>
        [Test]
        public void LendingService_BorrowBook_ShouldThrowFriendlyError_WhenConcurrencyConflictOccurs()
        {
            // Arrange
            LendingService service = new LendingService(
                this.mockLoanRepo.Object,
                this.mockReaderRepo.Object,
                this.mockCopyRepo.Object,
                this.mockBookRepo.Object,
                this.mockConfig.Object,
                this.mockLendingLogger.Object);

            this.mockConfig.SetupConfigDefaultLimits();

            Reader reader = LibraryTestFactory.CreateReader();
            BookCopy copy = LibraryTestFactory.CreateCopy();

            // Setup valid state
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(copy.Id, copy);
            this.mockCopyRepo.SetupFind(new[] { copy }); // Stock check

            // FORCE CONCURRENCY ERROR on Save
            this.mockLoanRepo.Setup(r => r.SaveChanges())
                .Throws(new DbUpdateConcurrencyException("RowVersion mismatch", new List<IUpdateEntry>()));

            // Act
            Action act = () => service.BorrowBook(reader.Id, copy.Id);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*taken by another user*");
        }
    }
}