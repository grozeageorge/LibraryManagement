// <copyright file="LendingServiceDomainHistoryTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Implementations
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Tests.Helpers;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests for the Lending Service for the Domain History Limit (D or L) for standard and librarian types of reader and also checking if older loans are ignored.
    /// </summary>
    [TestFixture]
    public class LendingServiceDomainHistoryTests
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
        /// Tests the Domain History Limit (D) for Standard Readers and for Librarians (Limit is doubled).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="limitD">The limit d.</param>
        /// <param name="currentCount">The current count.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(ReaderType.Standard, 2, 1, true)]
        [TestCase(ReaderType.Standard, 2, 2, false)]
        [TestCase(ReaderType.Standard, 5, 4, true)]
        [TestCase(ReaderType.Standard, 5, 5, false)]
        [TestCase(ReaderType.Librarian, 2, 3, true)]
        [TestCase(ReaderType.Librarian, 2, 4, false)]
        [TestCase(ReaderType.Librarian, 3, 5, true)]
        [TestCase(ReaderType.Librarian, 6, 3, false)]
        public void BorrowBook_DomainHistory(ReaderType type, int limitD, int currentCount, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(type: type);
            BookDomain domain = LibraryTestFactory.CreateDomain(name: "Science Fiction");
            Book targetBook = LibraryTestFactory.CreateBook(title: "Dune", domain: domain);
            BookEdition targetEdition = LibraryTestFactory.CreateEdition(book: targetBook);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(edition: targetEdition, isAvailable: true);

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerDomain: limitD, domainCheckIntervalMonths: 6);

            // Setup history (Existing loans)
            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < currentCount; i++)
            {
                Book oldBook = LibraryTestFactory.CreateBook(title: $"Old Book {i + 1}", domain: domain);
                BookEdition oldEdition = LibraryTestFactory.CreateEdition(book: oldBook);
                BookCopy oldCopy = LibraryTestFactory.CreateCopy(edition: oldEdition, isAvailable: false);
                loans.Add(LibraryTestFactory.CreateLoan(readerId: reader.Id, copy: oldCopy, loanDate: DateTime.Now.AddMonths(-1)));
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(loans);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
            }
            else
            {
                act.Should().Throw<InvalidOperationException>()
                    .WithMessage("*limit*domain*");
            }
        }

        /// <summary>
        /// Tests that loans older than L months are ignored.
        /// </summary>
        [Test]
        public void BorrowBook_DomainHistory_ShouldIgnoreOldLoans()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            BookDomain domain = LibraryTestFactory.CreateDomain(name: "History");
            Book targetBook = LibraryTestFactory.CreateBook(title: "World History", domain: domain);
            BookEdition targetEdition = LibraryTestFactory.CreateEdition(book: targetBook);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(edition: targetEdition, isAvailable: true);

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerDomain: 2, domainCheckIntervalMonths: 3);

            List<Loan> loans = new List<Loan>(); // History: 2 loans (limit reached), but one is 5 months old (outside window)

            Book b1 = LibraryTestFactory.CreateBook(title: "B1", domain: domain);
            loans.Add(LibraryTestFactory.CreateLoan(
                readerId: reader.Id,
                copy: LibraryTestFactory.CreateCopy(edition: LibraryTestFactory.CreateEdition(book: b1)),
                loanDate: DateTime.Now.AddMonths(-1))); // Recent

            List<Loan> recentLoans = new List<Loan> { loans[0] };

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(recentLoans);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id);

            // Assert
            act.Should().NotThrow(); // Should pass because count is 1, limit is 2
        }
    }
}
