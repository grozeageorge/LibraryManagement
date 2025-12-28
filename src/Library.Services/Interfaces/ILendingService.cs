// <copyright file="ILendingService.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Interfaces
{
    /// <summary>
    /// Interface for the business logic related to lending books.
    /// </summary>
    public interface ILendingService
    {
        /// <summary>
        /// Borrows the book.
        /// </summary>
        /// <param name="readerId">The reader identifier.</param>
        /// <param name="bookCopyId">The book copy identifier.</param>
        /// <param name="librarianId">The librarian identifier.</param>
        void BorrowBook(Guid readerId, Guid bookCopyId, Guid? librarianId = null);

        /// <summary>
        /// Returns the book.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        void ReturnBook(Guid loanId);

        /// <summary>
        /// Extends the due date of an active loan.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="days">The number of days to extend.</param>
        void ExtendLoan(Guid loanId, int days);

        /// <summary>
        /// Borrows multiple books in a single transaction. Enforces the "3 books must be from 2 categories" rule.
        /// </summary>
        /// <param name="readerId">The reader identifier.</param>
        /// <param name="bookCopyIds">List of book copy ids.</param>
        void BorrowBooks(Guid readerId, IEnumerable<Guid> bookCopyIds);
    }
}
