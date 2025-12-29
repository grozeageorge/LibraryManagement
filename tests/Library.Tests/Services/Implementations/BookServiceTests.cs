// <copyright file="BookServiceTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Implementations
{
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Tests.Helpers;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests for the book service class to assure the good communication of the repository's behaviour and the database.
    /// </summary>
    [TestFixture]
    public class BookServiceTests
    {
        private Mock<IRepository<Book>> mockRepository;
        private Mock<ILogger<BookService>> mockLogger;
        private Mock<IValidator<Book>> mockValidator;
        private Mock<ILibraryConfiguration> mockConfig;
        private BookService service;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockRepository = new Mock<IRepository<Book>>();
            this.mockLogger = new Mock<ILogger<BookService>>();
            this.mockValidator = new Mock<IValidator<Book>>();
            this.mockConfig = new Mock<ILibraryConfiguration>();
            this.service = new BookService(
                this.mockRepository.Object,
                this.mockLogger.Object,
                this.mockValidator.Object,
                this.mockConfig.Object);
        }

        /// <summary>
        /// Add book method should call repository when book is valid and it should add the book and save changes.
        /// </summary>
        [Test]
        public void AddBook_ShouldCallRepository_WhenBookIsValid()
        {
            // Arrange
            Book book = LibraryTestFactory.CreateBook("Test book");

            this.mockValidator.Setup(v => v.Validate(book))
                .Returns(new ValidationResult());

            this.mockConfig.SetupConfigDefaultLimits(maxBooksPerReader: 3);

            // Act
            this.service.AddBook(book);

            // Assert
            this.mockRepository.VerifyAdd(Times.Once());
            this.mockRepository.VerifySaved();
        }

        /// <summary>
        /// Add book method should throw argument exception when validation fails.
        /// </summary>
        [Test]
        public void AddBook_ShouldThrowArgumentException_WhenValidationFails()
        {
            // Arrange
            Book book = LibraryTestFactory.CreateBook(string.Empty);
            ValidationResult failure = new ValidationResult(new[] { new ValidationFailure("Title", "Required") });

            this.mockValidator.Setup(v => v.Validate(book)).Returns(failure);

            // Act
            Action act = () => this.service.AddBook(book);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Required*");
            this.mockRepository.VerifyAdd(Times.Never());
        }

        /// <summary>
        /// Add book method should throw invalid operation exception when the number of domains exceed the limit.
        /// </summary>
        [Test]
        public void AddBook_ShouldThrowInvalidOperationException_WhenDomainsExceedLimit()
        {
            // Arrange
            Book book = new Book { Title = "Book" };

            book.Domains.Add(new BookDomain { Name = "D1" });
            book.Domains.Add(new BookDomain { Name = "D2" });
            book.Domains.Add(new BookDomain { Name = "D3" });
            book.Domains.Add(new BookDomain { Name = "D4" });

            this.mockValidator.Setup(v => v.Validate(book)).Returns(new ValidationResult());
            this.mockConfig.SetupConfigDefaultLimits(maxDomainsPerBook: 3);

            // Act
            Action act = () => this.service.AddBook(book);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*exceeds maximum*");
        }

        /// <summary>
        /// Get book by identifier method should return a book when a book exists with that identifier.
        /// </summary>
        [Test]
        public void GetBookById_ShouldReturnBook_WhenBookExists()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            Book book = new Book { Id = id, Title = "Book" };

            this.mockRepository.SetupGetById(id, book);

            // Act
            Book? result = this.service.GetBookById(id);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Book");
        }

        /// <summary>
        /// Get all books method should return a list containing all the books in the library.
        /// </summary>
        [Test]
        public void GetAllBooks_ShouldReturnList()
        {
            // Arrange
            IEnumerable<Book> list = new List<Book>
            {
                new Book { Title = "B1" },
                new Book { Title = "B2" },
            };

            this.mockRepository.Setup(r => r.GetAll()).Returns(list);

            // Act
            IEnumerable<Book> result = this.service.GetAllBooks();
        }
    }
}
