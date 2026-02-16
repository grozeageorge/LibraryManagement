// <copyright file="LendingServiceReaderLimitsTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Implementations
{
    using FluentAssertions;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Tests.Helpers;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests for the lending service reader limits rule when borrowing books (NMC).
    /// </summary>
    [TestFixture]
    public class LendingServiceReaderLimitsTests
    {
        private Mock<IRepository<Loan>> mockLoanRepo;
        private Mock<IRepository<Reader>> mockReaderRepo;
        private Mock<IRepository<BookCopy>> mockCopyRepo;
        private Mock<IRepository<Book>> mockBookRepo;
        private Mock<ILibraryConfiguration> mockConfig;
        private Mock<ILogger<LendingService>> mockLogger;
        private LendingService lendingService;

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
            this.lendingService = new LendingService(
                this.mockLoanRepo.Object,
                this.mockReaderRepo.Object,
                this.mockCopyRepo.Object,
                this.mockBookRepo.Object,
                this.mockConfig.Object,
                this.mockLogger.Object);
        }

        /// <summary>
        /// Tests the Max Books Per Reader rule for both Reader types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="configLimit">The configuration limit.</param>
        /// <param name="currentLoans">The current loans.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(ReaderType.Standard, 3, 2, true)]
        [TestCase(ReaderType.Standard, 3, 3, false)]
        [TestCase(ReaderType.Librarian, 3, 3, true)]
        [TestCase(ReaderType.Librarian, 3, 5, true)]
        [TestCase(ReaderType.Librarian, 3, 6, false)]
        [TestCase(ReaderType.Standard, 1, 0, true)]
        [TestCase(ReaderType.Standard, 1, 1, false)]
        [TestCase(ReaderType.Standard, 10, 9, true)]
        [TestCase(ReaderType.Librarian, 1, 1, true)]
        [TestCase(ReaderType.Librarian, 1, 2, false)]
        [TestCase(ReaderType.Librarian, 5, 9, true)]
        public void BorrowBook_ReaderLimits_Boundaries(ReaderType type, int configLimit, int currentLoans, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(type: type);
            Book book = LibraryTestFactory.CreateBook("Math");
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book, bookType: "Educational", publisher: "P");
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: true);

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerReader: configLimit);

            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < currentLoans; i++)
            {
                loans.Add(LibraryTestFactory.CreateLoan(readerId: reader.Id));
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(loans);

            // Act
            Action act = () => this.lendingService.BorrowBook(reader.Id, targetCopy.Id);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
            }
            else
            {
                act.Should().Throw<InvalidOperationException>()
                    .WithMessage("*maximum number of borrowed books*");
            }
        }
    }
}
