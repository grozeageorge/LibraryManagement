// <copyright file="LendingServiceExtensionTests.cs" company="Transilvania University of Brasov">
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
    /// Tests in the lending service mainly for checking boundaries and failures for the extend loan method.
    /// </summary>
    [TestFixture]
    public class LendingServiceExtensionTests
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
        /// Extend loan should fail if a loan with a specific id is not found.
        /// </summary>
        [Test]
        public void ExtendLoan_ShouldThrow_WhenLoanNotFound()
        {
            // Arrange
            Guid loanId = Guid.NewGuid();
            this.mockLoanRepo.Setup(l => l.GetById(loanId)).Returns((Loan?)null);

            // Act
            Action act = () => this.service.ExtendLoan(loanId, 5);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Loan not found.");
        }

        /// <summary>
        /// Extend loan should fail when the days are 0 or negative.
        /// </summary>
        /// <param name="days">The days.</param>
        [TestCase(0)]
        [TestCase(-1)]
        public void ExtendLoan_ShouldThrow_WhenDaysInvalid(int days)
        {
            // Act
            Action act = () => this.service.ExtendLoan(Guid.NewGuid(), days);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*positive*");
        }

        /// <summary>
        /// Extend loan should fail if the loaned book is already returned.
        /// </summary>
        [Test]
        public void ExtendLoan_ShouldThrow_WhenBookAlreadyReturned()
        {
            // Arrange
            Loan loan = LibraryTestFactory.CreateLoan(returnDate: DateTime.Now);

            this.mockLoanRepo.SetupGetById(loan.Id, loan);

            // Act
            Action act = () => this.service.ExtendLoan(loan.Id, 5);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*returned*");
        }

        /// <summary>
        /// Extend loan should update due date when the loan is valid and the limits are respected.
        /// </summary>
        [Test]
        public void ExtendLoan_ShouldUpdateDueDate_WhenValid()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Loan loan = LibraryTestFactory.CreateLoan(readerId: reader.Id);
            DateTime initialDueDate = DateTime.Now.AddDays(10);
            loan.DueDate = initialDueDate;

            this.mockLoanRepo.SetupGetById(loan.Id, loan);
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockConfig.SetupConfigDefaultLimits(maxExtensionDays: 30);

            // Act
            this.service.ExtendLoan(loan.Id, 10);

            // Assert
            loan.ExtensionDaysCount.Should().Be(10);
            loan.DueDate.Should().Be(initialDueDate.AddDays(10));
            this.mockLoanRepo.Verify(l => l.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests the limits and bounderies of extend loan method.
        /// </summary>
        /// <param name="type">The reader type.</param>
        /// <param name="configLimit">The configuration limit.</param>
        /// <param name="currentExtensions">The current extensions.</param>
        /// <param name="newExtensions">The new extensions.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(ReaderType.Standard, 30, 20, 15, false)]
        [TestCase(ReaderType.Standard, 30, 20, 10, true)]
        [TestCase(ReaderType.Librarian, 30, 50, 10, true)]
        [TestCase(ReaderType.Librarian, 30, 50, 11, false)]
        public void ExtendLoan_LimitCheck_Boundaries(ReaderType type, int configLimit, int currentExtensions, int newExtensions, bool shouldSucceed)
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader(type: type);
            Loan loan = LibraryTestFactory.CreateLoan(readerId: reader.Id);
            loan.ExtensionDaysCount = currentExtensions;
            loan.DueDate = DateTime.Now;

            this.mockLoanRepo.SetupGetById(loan.Id, loan);
            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockConfig.SetupConfigDefaultLimits(maxExtensionDays: configLimit);

            // Act
            Action act = () => this.service.ExtendLoan(loan.Id, newExtensions);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
                loan.ExtensionDaysCount.Should().Be(currentExtensions + newExtensions);
            }
            else
            {
                act.Should().Throw<InvalidOperationException>().WithMessage("*exceed the limit*");
            }
        }
    }
}
