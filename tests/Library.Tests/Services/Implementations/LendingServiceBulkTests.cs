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
    using Library.Tests.Helpers;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests regarding limits (C) and correct functionality of borrowing a bulk of books from the library.
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
            Reader reader = LibraryTestFactory.CreateReader(type: type);

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerLoan: configLimit);

            List<Guid> copyIds = new List<Guid>();
            for (int i = 0; i < requestCount; i++)
            {
                BookCopy copy = LibraryTestFactory.CreateCopy();
                copyIds.Add(copy.Id);

                this.mockCopyRepo.SetupGetById(id: copy.Id, entity: copy);
                this.mockCopyRepo.SetupFind(results: new List<BookCopy> { copy });
            }

            this.mockReaderRepo.SetupGetById(id: reader.Id, entity: reader);

            // Act
            Action act = () => this.service.BorrowBooks(reader.Id, copyIds);

            // Assert
            if (shouldSucceed)
            {
                act.Should().NotThrow();
                this.mockLoanRepo.VerifyAdd(Times.Exactly(requestCount));
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
            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerLoan: 5);

            Reader reader = LibraryTestFactory.CreateReader();
            this.mockReaderRepo.SetupGetById(id: reader.Id, entity: reader);

            List<Guid> copyIds = new List<Guid>();
            BookDomain sharedDomain = LibraryTestFactory.CreateDomain("SingleDomain");
            for (int i = 0; i < 3; i++)
            {
                Book book = LibraryTestFactory.CreateBook(title: $"Book {i}", domain: sharedDomain);
                BookCopy copy = LibraryTestFactory.CreateCopy(book: book);
                copyIds.Add(copy.Id);

                this.mockCopyRepo.SetupGetById(id: copy.Id, entity: copy);
                this.mockCopyRepo.SetupFind(results: new List<BookCopy> { copy });
            }

            // Act
            Action act = () => this.service.BorrowBooks(reader.Id, copyIds);

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
            Reader reader = LibraryTestFactory.CreateReader();

            BookDomain d1 = LibraryTestFactory.CreateDomain("Science");
            BookDomain d2 = LibraryTestFactory.CreateDomain("Arts");
            BookDomain[] domainsToUse = new[] { d1, d1, d2 };

            List<Guid> copyIds = new List<Guid>();

            foreach (var domain in domainsToUse)
            {
                Book book = LibraryTestFactory.CreateBook(domain: domain);
                BookCopy copy = LibraryTestFactory.CreateCopy(book: book);

                copyIds.Add(copy.Id);

                this.mockCopyRepo.SetupGetById(copy.Id, copy);
                this.mockCopyRepo.SetupFind(new List<BookCopy> { copy });
            }

            this.mockReaderRepo.SetupGetById(reader.Id, reader);
            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerLoan: 5);

            // Act
            Action act = () => this.service.BorrowBooks(reader.Id, copyIds);

            // Assert
            act.Should().NotThrow();
        }
    }
}
