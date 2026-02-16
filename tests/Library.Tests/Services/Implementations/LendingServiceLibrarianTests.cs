// <copyright file="LendingServiceLibrarianTests.cs" company="Transilvania University of Brasov">
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
    /// Tests for the lending service related to librarian functionalities and limits (PERSIMP).
    /// </summary>
    [TestFixture]
    public class LendingServiceLibrarianTests
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
        /// Borrow book should throw when the librarian exceeds the daily processing limit.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenLibrarianExceedsDailyProcessingLimit()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(ReaderType.Standard);
            Reader librarian = LibraryTestFactory.CreateReader(ReaderType.Librarian);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy();

            this.mockConfig.SetupConfigDefaultLimits(maxProcessedPerDayLibrarian: 20);

            List<Loan> processedLoans = new List<Loan>();
            for (int i = 0; i < 19; i++)
            {
                processedLoans.Add(LibraryTestFactory.CreateLoan(librarian: librarian, loanDate: DateTime.Today));
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockReaderRepo.SetupGetById(librarian.Id, librarian);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(processedLoans);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id, librarian.Id);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*processing limit*");
        }

        /// <summary>
        /// Borrow book should pass when librarian loan limit is not reached.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldPass_WhenLibrarianIsUnderLimit()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(ReaderType.Standard);
            Reader librarian = LibraryTestFactory.CreateReader(ReaderType.Librarian);
            BookCopy targetCopy = LibraryTestFactory.CreateCopy();

            this.mockConfig.SetupConfigDefaultLimits(maxProcessedPerDayLibrarian: 20);

            List<Loan> processedLoans = new List<Loan>();
            for (int i = 0; i < 19; i++)
            {
                processedLoans.Add(LibraryTestFactory.CreateLoan(librarian: librarian, loanDate: DateTime.Today));
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockReaderRepo.SetupGetById(librarian.Id, librarian);
            this.mockCopyRepo.SetupGetById(targetCopy.Id, targetCopy);
            this.mockCopyRepo.SetupFind(new List<BookCopy> { targetCopy });
            this.mockLoanRepo.SetupFind(processedLoans);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, targetCopy.Id, librarian.Id);

            // Assert
            act.Should().NotThrow();
        }
    }
}
