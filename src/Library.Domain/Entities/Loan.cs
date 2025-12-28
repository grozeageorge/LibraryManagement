// <copyright file="Loan.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Loan entity that represents a transaction of borrowing a book copy by a reader.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class Loan : BaseEntity
    {
        /// <summary>
        /// Gets or sets the reader identifier.
        /// </summary>
        /// <value>
        /// The reader identifier.
        /// </value>
        public Guid ReaderId { get; set; }

        /// <summary>
        /// Gets or sets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        public virtual Reader? Reader { get; set; }

        /// <summary>
        /// Gets or sets the book copy identifier.
        /// </summary>
        /// <value>
        /// The book copy identifier.
        /// </value>
        public Guid BookCopyId { get; set; }

        /// <summary>
        /// Gets or sets the book copy.
        /// </summary>
        /// <value>
        /// The book copy.
        /// </value>
        public virtual BookCopy? BookCopy { get; set; }

        /// <summary>
        /// Gets or sets the date when the book has been borrowed.
        /// </summary>
        /// <value>
        /// The loan date.
        /// </value>
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the book is due for return.
        /// </summary>
        /// <value>
        /// The due date.
        /// </value>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the actual date when the book has been returned.
        /// </summary>
        /// <value>
        /// The return date.
        /// </value>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Gets or sets the count of how many days has the loan been extended.
        /// </summary>
        /// <value>
        /// The extension days count.
        /// </value>
        public int ExtensionDaysCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the librarian identifier.
        /// </summary>
        /// <value>
        /// The librarian identifier.
        /// </value>
        public Guid? LibrarianId { get; set; }

        /// <summary>
        /// Gets or sets the librarian.
        /// </summary>
        /// <value>
        /// The librarian.
        /// </value>
        public virtual Reader? Librarian { get; set; }
    }
}
