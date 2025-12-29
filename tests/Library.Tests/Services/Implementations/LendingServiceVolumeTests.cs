// <copyright file="LendingServiceVolumeTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using FluentAssertions;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Tests.Helpers;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Lending service volume tests for various limits and rules implemented in the lending service.
    /// </summary>
    [TestFixture]
    public class LendingServiceVolumeTests
    {
        private Mock<IRepository<Loan>> mockLoanRepo;
        private Mock<IRepository<Reader>> mockReaderRepo;
        private Mock<IRepository<BookCopy>> mockCopyRepo;
        private Mock<IRepository<Book>> mockBookRepo;
        private Mock<ILibraryConfiguration> mockConfig;
        private Mock<ILogger<LendingService>> mockLogger;
        private LendingService service;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockLoanRepo = new Mock<IRepository<Loan>>();
            this.mockReaderRepo = new Mock<IRepository<Reader>>();
            this.mockCopyRepo = new Mock<IRepository<BookCopy>>();
            this.mockBookRepo = new Mock<IRepository<Book>>();
            this.mockConfig = new Mock<ILibraryConfiguration>();
            this.mockLogger = new Mock<ILogger<LendingService>>();

            this.service = new LendingService(
                this.mockLoanRepo.Object,
                this.mockReaderRepo.Object,
                this.mockCopyRepo.Object,
                this.mockBookRepo.Object,
                this.mockConfig.Object,
                this.mockLogger.Object);
        }

        /// <summary>
        /// Stock rule volume tests, testing percentages from 0% to 19%, total copies = 100, if available is less than 10 it should fail else it should pass.
        /// </summary>
        /// <param name="availableCount">The available count.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, false)]
        [TestCase(5, false)]
        [TestCase(6, false)]
        [TestCase(7, false)]
        [TestCase(8, false)]
        [TestCase(9, false)]
        [TestCase(10, true)]
        [TestCase(11, true)]
        [TestCase(12, true)]
        [TestCase(13, true)]
        [TestCase(14, true)]
        [TestCase(15, true)]
        [TestCase(16, true)]
        [TestCase(17, true)]
        [TestCase(18, true)]
        [TestCase(19, true)]
        [TestCase(20, true)]
        [TestCase(21, true)]
        [TestCase(22, true)]
        [TestCase(23, true)]
        [TestCase(24, true)]
        [TestCase(25, true)]
        [TestCase(26, true)]
        [TestCase(27, true)]
        [TestCase(28, true)]
        [TestCase(29, true)]
        [TestCase(30, true)]
        [TestCase(31, true)]
        [TestCase(32, true)]
        [TestCase(33, true)]
        [TestCase(34, true)]
        [TestCase(35, true)]
        [TestCase(36, true)]
        [TestCase(37, true)]
        [TestCase(38, true)]
        [TestCase(39, true)]
        public void StockRule_Percentage_Sweep(int availableCount, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Book book = LibraryTestFactory.CreateBook();
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(edition: edition, isAvailable: true);

            // Create 100 copies total
            var allCopies = new List<BookCopy>();

            // Add the available ones
            // Ensure targetCopy is included if availableCount > 0
            if (availableCount > 0)
            {
                allCopies.Add(targetCopy);
                for (int i = 1; i < availableCount; i++)
                {
                    allCopies.Add(LibraryTestFactory.CreateCopy(edition: edition, isAvailable: true));
                }
            }

            // Fill the rest with borrowed copies
            int borrowedCount = 100 - availableCount;
            for (int i = 0; i < borrowedCount; i++)
            {
                allCopies.Add(LibraryTestFactory.CreateCopy(edition: edition, isAvailable: false));
            }

            // Mocks
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(allCopies);

            // Config defaults
            this.mockConfig.SetupConfigDefaultLimits();

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
            }
            else
            {
                act.Should().Throw<InvalidOperationException>().WithMessage("*Stock*");
            }
        }

        /// <summary>
        /// Extension limit volume tests, limit is 30 days, we add 1 to 20 days when we already have 20 days used.
        /// </summary>
        /// <param name="daysToAdd">The days to add.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(1, true)]
        [TestCase(5, true)]
        [TestCase(9, true)]
        [TestCase(10, true)]
        [TestCase(11, false)]
        [TestCase(12, false)]
        [TestCase(13, false)]
        [TestCase(14, false)]
        [TestCase(15, false)]
        [TestCase(16, false)]
        [TestCase(17, false)]
        [TestCase(18, false)]
        [TestCase(19, false)]
        [TestCase(20, false)]
        [TestCase(30, false)]
        [TestCase(40, false)]
        public void ExtendLoan_Days_Sweep(int daysToAdd, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Loan loan = LibraryTestFactory.CreateLoan(readerId: reader.Id);
            loan.ExtensionDaysCount = 20;
            loan.DueDate = DateTime.Now;

            this.mockLoanRepo.SetupGetById(loan.Id, loan);
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockConfig.SetupConfigDefaultLimits(maxExtensionDays: 30);

            // Act
            Action act = () => this.service.ExtendLoan(loan.Id, daysToAdd);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
            }
            else
            {
                act.Should().Throw<InvalidOperationException>().WithMessage("*exceed*");
            }
        }

        /// <summary>
        /// Reborrow (Delta) volume tests, delta is 10 days, we test returning X days ago.
        /// </summary>
        /// <param name="daysAgo">The days ago.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(1, false)]
        [TestCase(5, false)]
        [TestCase(9, false)]
        [TestCase(10, true)]
        [TestCase(11, true)]
        [TestCase(12, true)]
        [TestCase(15, true)]
        [TestCase(20, true)]
        [TestCase(30, true)]
        [TestCase(40, true)]
        [TestCase(50, true)]
        [TestCase(60, true)]
        [TestCase(70, true)]
        [TestCase(80, true)]
        [TestCase(90, true)]
        [TestCase(100, true)]
        public void Reborrow_DaysAgo_Sweep(int daysAgo, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Book book = LibraryTestFactory.CreateBook();
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(edition: edition, isAvailable: true);

            this.mockConfig.SetupConfigDefaultLimits(reborrowRestrictedDays: 10);

            Loan lastLoan = LibraryTestFactory.CreateLoan(
                readerId: reader.Id,
                copy: LibraryTestFactory.CreateCopy(),
                loanDate: DateTime.Now.AddDays(-daysAgo),
                returnDate: DateTime.Now.AddDays(-daysAgo + 1));

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(new List<Loan> { lastLoan });

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
            }
            else
            {
                act.Should().Throw<InvalidOperationException>().WithMessage("*wait*");
            }
        }

        /// <summary>
        /// Bulks borrow categories testing the 3 books 2 categories rule.
        /// </summary>
        /// <param name="bookCount">The book count.</param>
        /// <param name="categoryCount">The category count.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(1, 1, true)]
        [TestCase(2, 1, true)]
        [TestCase(2, 2, true)]
        [TestCase(3, 1, false)]
        [TestCase(3, 2, true)]
        [TestCase(3, 3, true)]
        [TestCase(4, 1, false)]
        [TestCase(4, 2, true)]
        [TestCase(5, 1, false)]
        [TestCase(5, 2, true)]
        [TestCase(6, 2, true)]
        [TestCase(6, 3, true)]
        [TestCase(7, 1, false)]
        public void BulkBorrow_Categories_Sweep(int bookCount, int categoryCount, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            var copyIds = new List<Guid>();

            // Create domains
            var domains = new List<BookDomain>();
            for (int i = 0; i < categoryCount; i++)
            {
                domains.Add(LibraryTestFactory.CreateDomain($"D{i}"));
            }

            // Create books and assign domains cyclically
            for (int i = 0; i < bookCount; i++)
            {
                var id = Guid.NewGuid();
                copyIds.Add(id);
                var domainToUse = domains[i % categoryCount]; // Distribute domains

                Book book = LibraryTestFactory.CreateBook(domain: domainToUse);
                BookEdition edition = LibraryTestFactory.CreateEdition(book: book);
                BookCopy copy = LibraryTestFactory.CreateCopy(edition: edition, isAvailable: true);

                this.mockCopyRepo.SetupGetById(id, copy);
                this.mockCopyRepo.SetupFind(new List<BookCopy> { copy });
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockConfig.SetupConfigDefaultLimits();

            // Act
            Action act = () => this.service.BorrowBooks(reader.Id, copyIds);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
            }
            else
            {
                act.Should().Throw<InvalidOperationException>().WithMessage("*distinct*");
            }
        }
    }
}