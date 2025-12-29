// <copyright file="LendingServiceParameterizedTests.cs" company="Transilvania University of Brasov">
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
    /// Prameterized tests for the Lending service class using TestCase attribute.
    /// </summary>
    [TestFixture]
    public class LendingServiceParameterizedTests
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
        /// Tests the 10% stock rule with various numbers.
        /// </summary>
        /// <param name="totalCopies">The total copies.</param>
        /// <param name="borrowedCount">The borrowed count.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(100, 91, false)]
        [TestCase(100, 90, true)]
        [TestCase(10, 9, true)]
        [TestCase(10, 10, false)]
        [TestCase(50, 46, false)]
        [TestCase(50, 45, true)]
        [TestCase(100, 0, true)]
        [TestCase(100, 50, true)]
        [TestCase(100, 80, true)]
        [TestCase(20, 18, true)]
        [TestCase(20, 19, false)]
        [TestCase(200, 181, false)]
        [TestCase(200, 180, true)]
        [TestCase(5, 4, true)]
        [TestCase(5, 5, false)]
        [TestCase(30, 28, false)]
        public void BorrowBook_StockRule_Boundaries(int totalCopies, int borrowedCount, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Book book = LibraryTestFactory.CreateBook("Math");
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book, bookType: "Educational", publisher: "P");
            BookCopy targetCopy = LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: true);

            // Generate the list of copies based on parameters
            List<BookCopy> allCopies = new List<BookCopy>();

            // Add borrowed copies
            for (int i = 0;  i < borrowedCount;  i++)
            {
                allCopies.Add(LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: false));
            }

            // Add available copies (Total - Borrowed) (I ensure targetcopy is one of the available ones if possible)
            int availableCount = totalCopies - borrowedCount;

            // If we expect success, one of the available copies MUST be our targetCopy
            if (availableCount > 0)
            {
                allCopies.Add(targetCopy);
                for (int i = 1; i < availableCount; i++)
                {
                    allCopies.Add(LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: true));
                }
            }
            else
            {
                // If 0 available, the target copy itself is theoretically not available or doesn't exist in the set
                // But for the logic of "ValidateStockRule", it counts ALL copies.
                // However, BorrowBook checks targetCopy.IsAvailable FIRST.
                // To test ONLY the Stock Rule logic, we must assume targetCopy IS available,
                // but the *calculation* of the rest of the stock fails.
                // So we add targetCopy as available, but maybe the total count makes it fail?
                // Actually, if targetCopy is available, availableCount is at least 1.
                // Let's stick to the math:
                // If TestCase says 100 total, 91 borrowed -> 9 available.
                for (int i = 0; i < availableCount; i++)
                {
                    allCopies.Add(LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: true));
                }

                // We must ensure targetCopy is returned by GetById
            }

            // Setup mocks
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(allCopies);
            this.mockConfig.SetupConfigDefaultLimits();

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
                    .WithMessage("*Stock is too low*");
            }
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
