// <copyright file="LibraryTestFactory.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Helpers
{
    using Library.Domain.Entities;

    /// <summary>
    /// Static factory for creating pre-configured domain entities for testing.
    /// </summary>
    public static class LibraryTestFactory
    {
        /// <summary>
        /// Creates the reader.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The reader respecting the type.</returns>
        public static Reader CreateReader(ReaderType type = ReaderType.Standard)
        {
            return new Reader
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                Email = "john@example.com",
                PhoneNumber = "555-1234",
                Type = type,
            };
        }

        /// <summary>
        /// Creates the domain.
        /// </summary>
        /// <param name="name">The name of the domain.</param>
        /// <returns>The book domain with the specified name or "General".</returns>
        public static BookDomain CreateDomain(string name = "General")
        {
            return new BookDomain
            {
                Id = Guid.NewGuid(),
                Name = name,
            };
        }

        /// <summary>
        /// Creates the book.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="domain">The domain.</param>
        /// <returns>
        /// The book respecting the parameters.
        /// </returns>
        public static Book CreateBook(string title = "Test Book", BookDomain? domain = null)
        {
            Book book = new Book
            {
                Id = Guid.NewGuid(),
                Title = title,
            };

            book.Domains.Add(domain ?? CreateDomain()); // Bypass AddDomain for simplicity (no config needed)

            return book;
        }

        /// <summary>
        /// Creates the book edition mainly used for testing.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="publisher">The publisher.</param>
        /// <param name="bookType">Type of the book.</param>
        /// <param name="pages">The pages.</param>
        /// <param name="year">The year.</param>
        /// <returns>A book edition with the specified parameters.</returns>
        public static BookEdition CreateEdition(
            Book? book = null,
            string publisher = "Test Publisher",
            string bookType = "Hardcover",
            int pages = 300,
            int year = 2025)
        {
            Book actualBook = book ?? CreateBook();

            return new BookEdition
            {
                Id = Guid.NewGuid(),
                Book = actualBook,
                BookId = actualBook.Id,
                Year = year,
                Publisher = publisher,
                BookType = bookType,
                NumberOfPages = pages,
            };
        }

        /// <summary>
        /// Creates the copy.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="edition">The edition of the book.</param>
        /// <param name="isAvailable">if set to <c>true</c> [is available].</param>
        /// <param name="isReadingRoomOnly">if set to <c>true</c> [is reading room only].</param>
        /// <returns>
        /// The copy respecting the parameters.
        /// </returns>
        public static BookCopy CreateCopy(
            Book? book = null,
            BookEdition? edition = null,
            bool isAvailable = true,
            bool isReadingRoomOnly = false)
        {
            Book actualBook = book ?? CreateBook();

            BookEdition actualEdition = edition ?? CreateEdition(actualBook);

            return new BookCopy
            {
                Id = Guid.NewGuid(),
                BookEdition = actualEdition,
                BookEditionId = actualEdition.Id,
                IsAvailable = isAvailable,
                IsReadingRoomOnly = isReadingRoomOnly,
            };
        }

        /// <summary>
        /// Creates the loan.
        /// </summary>
        /// <param name="readerId">The reader identifier.</param>
        /// <param name="copy">The copy.</param>
        /// <param name="loanDate">The loan date.</param>
        /// <param name="returnDate">The return date.</param>
        /// <param name="librarian">The librarian.</param>
        /// <returns>
        /// A loan respecting the parameters.
        /// </returns>
        public static Loan CreateLoan(
            Guid? readerId = null,
            BookCopy? copy = null,
            DateTime? loanDate = null,
            DateTime? returnDate = null,
            Reader? librarian = null)
        {
            BookCopy actualCopy = copy ?? CreateCopy();
            DateTime actualLoanDate = loanDate ?? DateTime.Now;

            return new Loan
            {
                Librarian = librarian,
                LibrarianId = librarian?.Id,
                Id = Guid.NewGuid(),
                ReaderId = readerId ?? Guid.NewGuid(),
                BookCopyId = actualCopy.Id,
                BookCopy = actualCopy,
                LoanDate = actualLoanDate,
                DueDate = actualLoanDate.AddDays(14),
                ReturnDate = returnDate,
                ExtensionDaysCount = 0,
            };
        }
    }
}
