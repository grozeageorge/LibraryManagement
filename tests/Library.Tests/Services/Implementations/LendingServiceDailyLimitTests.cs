// <copyright file="LendingServiceDailyLimitTests.cs" company="Transilvania University of Brasov">
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
    /// Tests for the Daily Limit (NCZ) for the different type of readers with different behaviours.
    /// </summary>
    [TestFixture]
    public class LendingServiceDailyLimitTests
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
        /// Tests to see if the Daily Limit (NCZ) is respected when standard readers borrow books.
        /// </summary>
        /// <param name="limitNCZ">The limit NCZ.</param>
        /// <param name="borrowedToday">The borrowed today.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(2, 1, true)]
        [TestCase(2, 2, false)]
        [TestCase(5, 0, true)]
        [TestCase(1, 1, false)]
        public void BorrowBook_DailyLimit_StandardReader(int limitNCZ, int borrowedToday, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            BookCopy targetCopy = LibraryTestFactory.CreateCopy();

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerDay: limitNCZ);

            // History: Loans from Today
            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < borrowedToday; i++)
            {
                loans.Add(LibraryTestFactory.CreateLoan(readerId: reader.Id, loanDate: DateTime.Today));
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
                    .WithMessage("*daily borrowing limit*");
            }
        }

        /// <summary>
        /// Tests if the Daily Limit (NCZ) is ignored for the librarian type of reader.
        /// </summary>
        [Test]
        public void BorrowBook_DailyLimit_ShouldBeIgnoredForLibrarian()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(type: ReaderType.Librarian);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy();

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerDay: 1);

            // History: Borrowed 5 books today (Way over limit)
            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < 5; i++)
            {
                loans.Add(LibraryTestFactory.CreateLoan(readerId: reader.Id, loanDate: DateTime.Today));
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(loans);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id);

            // Assert
            act.Should().NotThrow(); // Librarians ignore NCZ limit
        }
    }
}
