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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            BookCopy targetCopy = new BookCopy
            {
                Id = copyId,
                IsAvailable = true,
                BookEdition = new BookEdition
                {
                    Book = new Book { Title = "T" },
                    BookType = "Hardcover",
                    Publisher = "P",
                },
            };

            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(limitNCZ);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);
            this.mockConfig.Setup(c => c.ReborrowRestrictedDays).Returns(0);

            // History: Loans from Today
            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < borrowedToday; i++)
            {
                loans.Add(new Loan { ReaderId = readerId, LoanDate = DateTime.Today });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockCopyRepo.Setup(c => c.GetById(copyId)).Returns(targetCopy);

            this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepo.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(loans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            BookCopy targetCopy = new BookCopy
            {
                Id = copyId,
                IsAvailable = true,
                BookEdition = new BookEdition
                {
                    Book = new Book { Title = "T" },
                    BookType = "Hardcover",
                    Publisher = "P",
                },
            };

            // Limit is 1
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(1);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            // History: Borrowed 5 books today (Way over limit)
            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < 5; i++)
            {
                loans.Add(new Loan { ReaderId = readerId, LoanDate = DateTime.Today });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Librarian, FirstName = "Jane", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockCopyRepo.Setup(c => c.GetById(copyId)).Returns(targetCopy);

            this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepo.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(loans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().NotThrow(); // Librarians ignore NCZ limit
        }
    }
}
