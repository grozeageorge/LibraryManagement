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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns((BookCopy?)null);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            BookCopy copy = new BookCopy { Id = copyId, BookEdition = null };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(copy);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            Book book = new Book { Title = "Sample Book" };
            BookEdition edition = new BookEdition { Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy copy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = false };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(copy);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();

            Book book = new Book { Title = "Sample Book" };
            BookEdition edition = new BookEdition { Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy copy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true, IsReadingRoomOnly = true };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com" });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(copy);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

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
            Guid loanId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Loan loan = new Loan
            {
                Id = loanId,
                BookCopyId = copyId,
                ReturnDate = null,
            };

            BookCopy copy = new BookCopy { Id = copyId, IsAvailable = false };

            this.mockLoanRepository.Setup(l => l.GetById(loanId)).Returns(loan);
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(copy);

            // Act
            this.service.ReturnBook(loanId);

            // Assert
            loan.ReturnDate.Should().NotBeNull();
            copy.IsAvailable.Should().BeTrue();
            this.mockLoanRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Borrow book method should throw when the stock is below 10 percent.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenStockIsBelow10Percent()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid bookId = Guid.NewGuid();

            Book book = new Book { Id = bookId, Title = "Popular Book" };
            BookEdition edition = new BookEdition { BookId = bookId, Book = book, Publisher = "P", BookType = "Hardcover" };

            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            List<BookCopy> allCopies = new List<BookCopy>();
            for (int i = 0; i < 91; i++)
            {
                allCopies.Add(new BookCopy { IsAvailable = false, BookEdition = edition });
            }

            for (int i = 0; i < 9; i++)
            {
                allCopies.Add(new BookCopy { IsAvailable = true, BookEdition = edition });
            }

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com", Type = ReaderType.Standard });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(targetCopy);
            this.mockBookCopyRepository.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(allCopies);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*stock is too low*");
        }

        /// <summary>
        /// Borrow book method should pass when the stock is exactly at 10 percent.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldPass_WhenStockIsExactly10Percent()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid bookId = Guid.NewGuid();

            Book book = new Book { Id = bookId, Title = "Popular Book" };
            BookEdition edition = new BookEdition { BookId = bookId, Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            List<BookCopy> allCopies = new List<BookCopy>();
            for (int i = 0; i < 9; i++)
            {
                allCopies.Add(new BookCopy { IsAvailable = false, BookEdition = edition });
            }

            allCopies.Add(targetCopy); // 1 available copy

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com", Type = ReaderType.Standard });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(targetCopy);
            this.mockBookCopyRepository.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(allCopies);

            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(5);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(2);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(2);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(3);
            this.mockConfig.Setup(c => c.ReborrowRestrictedDays).Returns(90);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().NotThrow();
            this.mockLoanRepository.Verify(r => r.Add(It.IsAny<Loan>()), Times.Once);
        }

        /// <summary>
        /// Borrow book method should throw when the standard reader reached the maximum books limit.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenReaderReachedMaxBooksLimit()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid bookId = Guid.NewGuid();

            Book book = new Book { Id = bookId, Title = "Some Book" };
            BookEdition edition = new BookEdition { BookId = bookId, Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(3);

            List<Loan> existingLoans = new List<Loan>
            {
                new Loan { ReaderId = readerId, ReturnDate = null },
                new Loan { ReaderId = readerId, ReturnDate = null },
                new Loan { ReaderId = readerId, ReturnDate = null },
            };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com", Type = ReaderType.Standard });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(targetCopy);

            this.mockBookCopyRepository.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepository.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(existingLoans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*maximum number of borrowed books*");
        }

        /// <summary>
        /// Borrow book method should pass when the reader is a librarian and the reader exceeds standard limit.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldPass_WhenLibrarianExceedsStandardLimit()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid bookId = Guid.NewGuid();

            Book book = new Book { Id = bookId, Title = "Some Book" };
            BookEdition edition = new BookEdition { BookId = bookId, Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(3);
            this.mockConfig.Setup(c => c.MaxBooksPerDomain).Returns(2);
            this.mockConfig.Setup(c => c.DomainCheckIntervalMonths).Returns(3);
            this.mockConfig.Setup(c => c.ReborrowRestrictedDays).Returns(90);

            List<Loan> existingLoans = new List<Loan>
            {
                new Loan { ReaderId = readerId, ReturnDate = null },
                new Loan { ReaderId = readerId, ReturnDate = null },
                new Loan { ReaderId = readerId, ReturnDate = null },
                new Loan { ReaderId = readerId, ReturnDate = null },
            };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "Jane", LastName = "Smith", Address = "456 Elm St.", Email = "a@a.com", Type = ReaderType.Librarian });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(targetCopy);
            this.mockBookCopyRepository.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });
            this.mockLoanRepository.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(existingLoans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().NotThrow();
            this.mockLoanRepository.Verify(r => r.Add(It.IsAny<Loan>()), Times.Once);
        }

        /// <summary>
        /// Borrow book method should throw when the daily limit is reached.
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenDailyLimitReached()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid bookId = Guid.NewGuid();

            Book book = new Book { Id = bookId, Title = "Some Book" };
            BookEdition edition = new BookEdition { BookId = bookId, Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(10);
            this.mockConfig.Setup(c => c.MaxBooksPerDay).Returns(2);

            List<Loan> existingLoans = new List<Loan>
            {
                new Loan { ReaderId = readerId, LoanDate = DateTime.Today },
                new Loan { ReaderId = readerId, LoanDate = DateTime.Today },
            };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com", Type = ReaderType.Standard });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(targetCopy);

            this.mockBookCopyRepository.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });

            this.mockLoanRepository.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(existingLoans);

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*daily borrowing limit*");
        }

        /// <summary>
        /// Borrow book method should throw when reborrowing too soon (90 day rule).
        /// </summary>
        [Test]
        public void BorrowBook_ShouldThrow_WhenReborrowingTooSoon()
        {
            // Arrange
            Guid readerId = Guid.NewGuid();
            Guid copyId = Guid.NewGuid();
            Guid bookId = Guid.NewGuid();

            Book book = new Book { Id = bookId, Title = "Some Book" };
            BookEdition edition = new BookEdition { BookId = bookId, Book = book, Publisher = "P", BookType = "Hardcover" };
            BookCopy targetCopy = new BookCopy { Id = copyId, BookEdition = edition, IsAvailable = true };

            this.mockConfig.Setup(c => c.MaxBooksPerReader).Returns(5);
            this.mockConfig.Setup(c => c.ReborrowRestrictedDays).Returns(90);

            Loan lastLoan = new Loan
            {
                ReaderId = readerId,
                LoanDate = DateTime.Now.AddDays(-10),
                ReturnDate = DateTime.Now.AddDays(-5),
                BookCopy = new BookCopy { BookEdition = edition },
            };

            this.mockReaderRepository.Setup(r => r.GetById(readerId)).Returns(new Reader { FirstName = "John", LastName = "Doe", Address = "123 Main St.", Email = "a@a.com", Type = ReaderType.Standard });
            this.mockBookCopyRepository.Setup(c => c.GetById(copyId)).Returns(targetCopy);

            this.mockBookCopyRepository.Setup(c => c.Find(It.IsAny<Expression<Func<BookCopy, bool>>>()))
                .Returns(new List<BookCopy> { targetCopy });
            this.mockLoanRepository.Setup(l => l.Find(It.IsAny<Expression<Func<Loan, bool>>>()))
                .Returns(new List<Loan> { lastLoan });

            // Act
            Action act = () => this.service.BorrowBook(readerId, copyId);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
