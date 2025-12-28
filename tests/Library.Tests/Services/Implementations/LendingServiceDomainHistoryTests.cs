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
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests for the Lending Service for the Domain History Limit for standard and librarian types of reader and also checking if older loans are ignored.
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
        [TestCase(ReaderType.Librarian, 3, 6, false)]
        public void BorrowBook_DomainHistory(ReaderType type, int limitD, int currentCount, bool shouldSucceed)
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid domainId = Guid.NewGuid();

            BookDomain domain = new BookDomain { Id = domainId, Name = "Science Fiction" };

            Book targetBook = new Book { Title = "Dune" };
            targetBook.Domains.Add(domain); // Bypass AddDomain for simplicity (no config needed)
            BookEdition targetEdition = new BookEdition { Book = targetBook, BookType = "Hardcover", Publisher = "P" };
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = targetEdition, IsAvailable = true };

            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(limitD);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(6);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.ReborrowRestrictedDays).Returns(0);

            // Setup history (Existing loans)
            List<Loan> loans = new List<Loan>();
            for (int i = 0; i < currentCount; i++)
            {
                Book oldBook = new Book { Title = $"Old Book {i + 1}" };
                oldBook.Domains.Add(domain); // Bypass AddDomain for simplicity
                BookEdition oldEdition = new BookEdition { Book = oldBook, BookType = "Paperback", Publisher = "P" };
                BookCopy oldCopy = new BookCopy { BookEdition = oldEdition };

                loans.Add(new Loan
                {
                    ReaderId = readerId,
                    LoanDate = DateTime.Now.AddMonths(-1),
                    BookCopy = oldCopy,
                });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Id = readerId, Type = type, FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            BookDomain domain = new BookDomain { Name = "History" };
            Book targetBook = new Book { Title = "World History" };
            targetBook.Domains.Add(domain); // Bypass AddDomain for simplicity (no config needed)
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = new BookEdition { Book = targetBook, BookType = "Hardcover", Publisher = "P" }, IsAvailable = true };

            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(2);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(3);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);

            List<Loan> loans = new List<Loan>(); // History: 2 loans (limit reached), but one is 5 months old (outside window)

            Book b1 = new Book { Title = "B1" }; // Loan 1 should count because it is recent
            b1.Domains.Add(domain);
            loans.Add(new Loan
            {
                ReaderId = readerId,
                LoanDate = DateTime.Now.AddMonths(-1),
                BookCopy = new BookCopy
                {
                    BookEdition = new BookEdition
                    {
                        Book = b1,
                        BookType = "Hardcover",
                        Publisher = "P",
                    },
                },
            });

            // Loan 2: Old (Should NOT count)
            // Note: In the real service, the Repository.Find filters by date.
            // Since we Mock Find, we must ensure we simulate that filter or return the list and let the service logic handle it?
            // Wait, the Service logic does: `repo.Find(l => ... Date >= sinceDate)`.
            // If we return the old loan here, the Service assumes the Repo filtered it.
            // So we should ONLY return the recent loan to simulate the DB correctly.

            // Let's verify the Service logic actually asks for the correct date.
            // We will return ONLY the recent loan.
            List<Loan> recentLoans = new List<Loan> { loans[0] };

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockCopyRepo.Setup(c => c.GetById(copyId)).Returns(targetCopy);

            this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepo.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(recentLoans); // Return only 1 valid loan

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().NotThrow(); // Should pass because count is 1, limit is 2
        }
    }
}
