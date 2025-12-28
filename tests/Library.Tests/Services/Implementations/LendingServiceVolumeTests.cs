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
            var readerId = Guid.NewGuid();
            var copyId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var book = new Book { Id = bookId, Title = "T" };
            var edition = new BookEdition { BookId = bookId, Book = book, BookType = "H", Publisher = "P" };
            var targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            // Create 100 copies total
            var allCopies = new List<BookCopy>();

            // Add the available ones
            // Ensure targetCopy is included if availableCount > 0
            if (availableCount > 0)
            {
                allCopies.Add(targetCopy);
                for (int i = 1; i < availableCount; i++)
                {
                    allCopies.Add(new BookCopy { IsAvailable = true, BookEdition = edition });
                }
            }

            // Fill the rest with borrowed copies
            int borrowedCount = 100 - availableCount;
            for (int i = 0; i < borrowedCount; i++)
            {
                allCopies.Add(new BookCopy { IsAvailable = false, BookEdition = edition });
            }

            // Mocks
            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "A", LastName = "B", Address = "C", Email = "a@a.com" });
            this.mockCopyRepo.Setup(r => r.GetById(copyId)).Returns(targetCopy);
            this.mockCopyRepo.Setup(r => r.Find(It.IsAny<Expression<Func<BookCopy, bool>>>())).Returns(allCopies);

            // Config defaults
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            var loanId = Guid.NewGuid();
            var readerId = Guid.NewGuid();
            var loan = new Loan { Id = loanId, ReaderId = readerId, ExtensionDaysCount = 20, ReturnDate = null, DueDate = DateTime.Now };

            this.mockLoanRepo.Setup(r => r.GetById(loanId)).Returns(loan);
            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "A", LastName = "B", Address = "C", Email = "a@a.com" });
            this.mockConfig.Setup(c => c.MaxExtensionDays).Returns(30);

            // Act
            Action act = () => this.service.ExtendLoan(loanId, daysToAdd);

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
            var readerId = Guid.NewGuid();
            var copyId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, Title = "B" };
            var edition = new BookEdition { BookId = bookId, Book = book, BookType = "H", Publisher = "P" };
            var targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            this.mockConfig.Setup(c => c.ReborrowRestrictedDays).Returns(10);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            var lastLoan = new Loan
            {
                ReaderId = readerId,
                LoanDate = DateTime.Now.AddDays(-daysAgo), // Borrowed X days ago
                ReturnDate = DateTime.Now.AddDays(-daysAgo + 1),
                BookCopy = new BookCopy { BookEdition = edition },
            };

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "A", Address = "C", LastName = "B", Email = "a@a.com" });
            this.mockCopyRepo.Setup(r => r.GetById(copyId)).Returns(targetCopy);
            this.mockCopyRepo.Setup(r => r.Find(It.IsAny<Expression<Func<BookCopy, bool>>>())).Returns(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Loan, bool>>>())).Returns(new List<Loan> { lastLoan });

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            var readerId = Guid.NewGuid();
            var copyIds = new List<Guid>();

            // Create domains
            var domains = new List<BookDomain>();
            for (int i = 0; i < categoryCount; i++)
            {
                domains.Add(new BookDomain { Name = $"D{i}" });
            }

            // Create books and assign domains cyclically
            for (int i = 0; i < bookCount; i++)
            {
                var id = Guid.NewGuid();
                copyIds.Add(id);
                var domainToUse = domains[i % categoryCount]; // Distribute domains

                var copy = new BookCopy
                {
                    Id = id,
                    IsAvailable = true,
                    BookEdition = new BookEdition { Book = new Book { Title = "T", Domains = { domainToUse } }, BookType = "H", Publisher = "P" },
                };
                this.mockCopyRepo.Setup(r => r.GetById(id)).Returns(copy);
                this.mockCopyRepo.Setup(r => r.Find(It.IsAny<Expression<Func<BookCopy, bool>>>())).Returns(new List<BookCopy> { copy });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, Address = "C", FirstName = "A", LastName = "B", Email = "a@a.com" });
            this.mockConfig.Setup(c => c.MaxBooksPerLoan).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            // Act
            Action act = () => this.service.BorrowBooks(readerId, copyIds);

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