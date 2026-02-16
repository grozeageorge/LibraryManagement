// <copyright file="LendingServiceTests.cs" company="Transilvania University of Brasov">
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
    /// Tests for the lending service to make sure the borrowing and returning of books works as expected.
    /// </summary>
    [TestFixture]
    public class LendingServiceTests
    {
        private Mock<IRepository<Loan>> mockLoanRepository;
        private Mock<IRepository<Reader>> mockReaderRepository;
        private Mock<IRepository<BookCopy>> mockBookCopyRepository;
        private Mock<IRepository<Book>> mockBookRepository;
        private Mock<ILibraryConfiguration> mockConfig;
        private Mock<ILogger<LendingService>> mockLogger;
        private LendingService service;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockLoanRepository = new Mock<IRepository<Loan>>();
            this.mockReaderRepository = new Mock<IRepository<Reader>>();
            this.mockBookCopyRepository = new Mock<IRepository<BookCopy>>();
            this.mockBookRepository = new Mock<IRepository<Book>>();
            this.mockConfig = new Mock<ILibraryConfiguration>();
            this.mockLogger = new Mock<ILogger<LendingService>>();
            this.service = new LendingService(
                this.mockLoanRepository.Object,
                this.mockReaderRepository.Object,
                this.mockBookCopyRepository.Object,
                this.mockBookRepository.Object,
                this.mockConfig.Object,
                this.mockLogger.Object);
        }

        /// <summary>
        /// Borrow book method should throw when a reader is not found.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenReaderNotFound()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns((Reader?)null);

            // Act
            Action act = () => this.service.BorrowBook(readerId, Guid.NewGuid());

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Reader not found.");
        }

        /// <summary>
        /// Borrow book method should throw when a copy of the book is not found.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenCopyNotFound()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Guid copyId = Guid.NewGuid();

            this.mockReaderRepository.SetupGetById(reader.Id, reader);
            this.mockBookCopyRepository.SetupGetById(copyId, (BookCopy?)null);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, copyId);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Book copy not found.");
        }

        /// <summary>
        /// Borrow book method should throw when the book hierarchy is incomplete (book edition is missing).
        /// </summary>
        [Test]
        public void BorrowBook_shouldThrow_WhenBookHierarchyIncomplete()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            BookCopy copy = LibraryTestFactory.CreateCopy(edition: null);

            this.mockReaderRepository.SetupGetById(reader.Id, reader);
            this.mockBookCopyRepository.SetupGetById(copy.Id, copy);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, copy.Id);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        /// <summary>
        /// Borrow book method should throw when the book copy is not available.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenCopyNotAvailable()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Book book = LibraryTestFactory.CreateBook();
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book);
            BookCopy copy = LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: false);

            this.mockReaderRepository.SetupGetById(reader.Id, reader);
            this.mockBookCopyRepository.SetupGetById(copy.Id, copy);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, copy.Id);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*already borrowed*");
        }

        /// <summary>
        /// Borrow book method should throw when the copy of the book is reading room only.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenCopyIsReadingRoomOnly()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();
            Book book = LibraryTestFactory.CreateBook();
            BookEdition edition = LibraryTestFactory.CreateEdition(book: book);
            BookCopy copy = LibraryTestFactory.CreateCopy(book: book, edition: edition, isAvailable: true, isReadingRoomOnly: true);

            this.mockReaderRepository.SetupGetById(reader.Id, reader);
            this.mockBookCopyRepository.SetupGetById(copy.Id, copy);

            // Act
            Action act = () => this.service.BorrowBook(reader.Id, copy.Id);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*reading room*");
        }

        /// <summary>
        /// Return book method should update return date when the loan is valid.
        /// </summary>
        [Test]
        public void ReturnBook_ShouldUpdateReturnDate_WhenValid()
        {
            // Arrange
            BookCopy copy = LibraryTestFactory.CreateCopy(isAvailable: false);
            Loan loan = LibraryTestFactory.CreateLoan(copy: copy, returnDate: null);

            this.mockLoanRepository.SetupGetById(loan.Id, loan);
            this.mockBookCopyRepository.SetupGetById(copy.Id, copy);

            // Act
            this.service.ReturnBook(loan.Id);

            // Assert
            loan.ReturnDate.Should().NotBeNull();
            copy.IsAvailable.Should().BeTrue();
            this.mockLoanRepository.VerifySaved();
        }
    }
}
