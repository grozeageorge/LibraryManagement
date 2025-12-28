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
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests for the lending service related to librarian functionalities and limits.
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
            Guid readerId = Guid.NewGuid();
            Guid librarianId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            BookCopy targetCopy = new BookCopy
            {
                Id = copyId,
                IsAvailable = true,
                BookEdition = new BookEdition
                {
                    Book = new Book { Title = "Sample Book" },
                    BookType = "H",
                    Publisher = "P",
                },
            };

            this.mockConfig.Setup(c => c.MaxProcessedPerDayLibrarian).Returns(20);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            List<Loan> processedLoans = new List<Loan>();
            for (int i = 0; i < 20; i++)
            {
                processedLoans.Add(new Loan
                {
                    LibrarianId = librarianId,
                    LoanDate = DateTime.Today,
                });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, Address = "C", FirstName = "A", LastName = "B", Email = "a@a.com" });
            this.mockReaderRepo.Setup(r => r.GetById(librarianId)).Returns(new Reader { Type = ReaderType.Librarian, Address = "C", FirstName = "L", LastName = "M", Email = "b@b.com" });

            this.mockCopyRepo.Setup(c => c.GetById(copyId)).Returns(targetCopy);
            this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepo.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(processedLoans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId, librarianId);

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
            Guid readerId = Guid.NewGuid();
            Guid librarianId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            BookCopy targetCopy = new BookCopy
            {
                Id = copyId,
                IsAvailable = true,
                BookEdition = new BookEdition
                {
                    Book = new Book { Title = "Sample Book" },
                    BookType = "H",
                    Publisher = "P",
                },
            };

            this.mockConfig.Setup(c => c.MaxProcessedPerDayLibrarian).Returns(20);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            List<Loan> processedLoans = new List<Loan>();
            for (int i = 0; i < 19; i++)
            {
                processedLoans.Add(new Loan
                {
                    LibrarianId = librarianId,
                    LoanDate = DateTime.Today,
                });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, Address = "C", FirstName = "A", LastName = "B", Email = "a@a.com" });
            this.mockReaderRepo.Setup(r => r.GetById(librarianId)).Returns(new Reader { Type = ReaderType.Librarian, Address = "C", FirstName = "L", LastName = "M", Email = "b@b.com" });

            this.mockCopyRepo.Setup(c => c.GetById(copyId)).Returns(targetCopy);
            this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepo.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(processedLoans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId, librarianId);

            // Assert
            act.Should().NotThrow();
        }
    }
}
