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
        void BorrowBook(Guid readerId, Guid bookCopyId);

        /// <summary>
        /// Returns the book.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        void ReturnBook(Guid loanId);
    }
}
