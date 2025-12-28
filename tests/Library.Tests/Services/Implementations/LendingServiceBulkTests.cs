// <copyright file="LendingServiceBulkTests.cs" company="Transilvania University of Brasov">
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
    /// Tests regarding limits and correct functionality of borrowing a bulk of books from the library.
    /// </summary>
    [TestFixture]
    public class LendingServiceBulkTests
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
        /// Test to see if the method is throwing correctly when the list of copyIds is empty.
        /// </summary>
        [Test]
        public void BorrowBooks_ShouldThrow_WhenListIsEmpty()
        {
            // Act
            Action act = () => this.service.BorrowBooks(Guid.NewGuid(), new List<Guid>());

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*at least one*");
        }

        /// <summary>
        /// Limit boundaries regarding the type, config and request for borrowing a bulk of books.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="configLimit">The configuration limit.</param>
        /// <param name="requestCount">The request count.</param>
        /// <param name="shouldSucceed">if set to <c>true</c> [should succeed].</param>
        [TestCase(ReaderType.Standard, 3, 4, false)]
        [TestCase(ReaderType.Standard, 3, 3, true)]
        [TestCase(ReaderType.Librarian, 3, 6, true)]
        [TestCase(ReaderType.Librarian, 3, 7, false)]
        public void BorrowBooks_Limit_Boundaries(ReaderType type, int configLimit, int requestCount, bool shouldSucceed)
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Reader reader = new Reader { Id = readerId, Type = type, FirstName = "A", LastName = "B", Address = "C", Email = "a@a.com" };

            this.mockConfig.Setup(c => c.MaxBooksPerLoan).Returns(configLimit);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            List<Guid> copyIds = new List<Guid>();
            for (int i = 0; i < requestCount; i++)
            {
                Guid id = Guid.NewGuid();
                copyIds.Add(id);

                BookCopy copy = new BookCopy
                {
                    Id = id,
                    IsAvailable = true,
                    BookEdition = new BookEdition
                    {
                        Book = new Book
                        {
                            Title = "T",
                            Domains = { new BookDomain { Name = "D1" }, new BookDomain { Name = "D2" } },
                        },
                        BookType = "H",
                        Publisher = "P",
                    },
                };

                this.mockCopyRepo.Setup(c => c.GetById(id)).Returns(copy);
                this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                    .Returns(new List<BookCopy> { copy });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(reader);

            // Act
            Action act = () => this.service.BorrowBooks(readerId, copyIds);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
                this.mockLoanRepo.Verify(l => l.Add(It.IsAny<Loan>()), Times.Exactly(requestCount));
            }
            else
            {
                act.Should().Throw<InvalidOperationException>().WithMessage("*more than*");
            }
        }

        /// <summary>
        /// When borrowing 3 books from 1 category the method should throw an exception because it breaks the rule.
        /// </summary>
        [Test]
        public void BorrowBooks_ShouldThrow_When3BooksFrom1Category()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            BookDomain domain = new BookDomain { Id = Guid.NewGuid(), Name = "SingleDomain" };

            List<Guid> copyIds = new List<Guid>();
            for (int i = 0; i < 3; i++)
            {
                Guid id = Guid.NewGuid();
                copyIds.Add(id);

                BookCopy copy = new BookCopy
                {
                    Id = id,
                    IsAvailable = true,
                    BookEdition = new BookEdition
                    {
                        Book = new Book
                        {
                            Title = "Book",
                            Domains = { domain },
                        },
                        BookType = "H",
                        Publisher = "P",
                    },
                };

                this.mockCopyRepo.Setup(c => c.GetById(id)).Returns(copy);
                this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                    .Returns(new List<BookCopy> { copy });
            }

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "A", LastName = "B", Address = "C", Email = "a@a.com" });
            this.mockConfig.Setup(c => c.MaxBooksPerLoan).Returns(5);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            // Act
            Action act = () => this.service.BorrowBooks(readerId, copyIds);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*distinct categories*");
        }

        /// <summary>
        /// Test to see if borrow books method works accordingly when we have 3 books from 2 different categories.
        /// </summary>
        [Test]
        public void BorrowBooks_ShouldPass_When3BooksFrom2Categories()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            BookDomain d1 = new BookDomain { Id = Guid.NewGuid(), Name = "D1" };
            BookDomain d2 = new BookDomain { Id = Guid.NewGuid(), Name = "D2" };

            List<Guid> copyIds = new List<Guid>();

            Guid id1 = Guid.NewGuid();
            copyIds.Add(id1);
            this.SetupCopy(id1, d1);

            Guid id2 = Guid.NewGuid();
            copyIds.Add(id2);
            this.SetupCopy(id2, d1);

            Guid id3 = Guid.NewGuid();
            copyIds.Add(id3);
            this.SetupCopy(id3, d2);

            this.mockReaderRepo.Setup(r => r.GetById(readerId)).Returns(new Reader { Type = ReaderType.Standard, FirstName = "A", Address = "C", LastName = "B", Email = "a@a.com" });
            this.mockConfig.Setup(c => c.MaxBooksPerLoan).Returns(5);
            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(100);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(100);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(1);

            // Act
            Action act = () => this.service.BorrowBooks(readerId, copyIds);

            // Assert
            act.Should().NotThrow();
        }

        private void SetupCopy(Guid id, BookDomain domain)
        {
            BookCopy copy = new BookCopy
            {
                Id = id,
                IsAvailable = true,
                BookEdition = new BookEdition
                {
                    Book = new Book
                    {
                        Title = "B",
                        Domains = { domain },
                    },
                    BookType = "H",
                    Publisher = "P",
                },
            };

            this.mockCopyRepo.Setup(c => c.GetById(id)).Returns(copy);
            this.mockCopyRepo.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { copy });
        }
    }
}
