using FluentAssertions;
using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Domain.Repositories;
using Library.Services.Implementations;
using Library.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Tests.Services.Implementations
{
    /// <summary>
    /// Tests for the lending service reborrow rule when borrowing books (DELTA).
    /// </summary>
    [TestFixture]
    public class LendingServiceReborrowTests
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
        /// Tests the Reborrow Delta (90 days) rule.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="configDelta">The configuration delta.</param>
        /// <param name="daysSinceLoan">The days since loan.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(ReaderType.Standard, 90, 89, false)]
        [TestCase(ReaderType.Standard, 90, 91, true)]
        [TestCase(ReaderType.Librarian, 90, 44, false)]
        [TestCase(ReaderType.Librarian, 90, 46, true)]
        [TestCase(ReaderType.Standard, 10, 1, false)]
        [TestCase(ReaderType.Standard, 10, 11, true)]
        [TestCase(ReaderType.Standard, 30, 15, false)]
        [TestCase(ReaderType.Librarian, 10, 4, false)]
        [TestCase(ReaderType.Librarian, 10, 6, true)]
        [TestCase(ReaderType.Librarian, 60, 29, false)]
        public void BorrowBook_ReborrowDelta_Boundaries(ReaderType type, int configDelta, int daysSinceLoan, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(type: type);
            Book book = LibraryTestFactory.CreateBook("Math");
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book, bookType: "Educational", publisher: "P");
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: true);

            this.mockConfig.SetupConfigDefaultLimits(reborrowRestrictedDays: configDelta);

            Loan lastLoan = LibraryTestFactory.CreateLoan(readerId: reader.Id, loanDate: DateTime.Now.AddDays(-daysSinceLoan), returnDate: DateTime.Now.AddDays(-daysSinceLoan + 5), copy: LibraryTestFactory.CreateCopy(edition: edition));

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(new List<Loan> { lastLoan });

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
                    .WithMessage("*Must wait*");
            }
        }
    }
}
