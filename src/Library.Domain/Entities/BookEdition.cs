// <copyright file="BookEdition.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Book edition entity that represents a specific edition of a book in the library system.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class BookEdition : BaseEntity
    {
        /// <summary>
        /// Gets or sets the publisher of the book.
        /// </summary>
        /// <value>
        /// The publisher.
        /// </value>
        required public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the year this edition published.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the edition number of the book.
        /// </summary>
        /// <value>
        /// The edition number.
        /// </value>
        public int EditionNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of pages of the book.
        /// </summary>
        /// <value>
        /// The number of pages.
        /// </value>
        public int NumberOfPages { get; set; }

        /// <summary>
        /// Gets or sets the type of the book.
        /// </summary>
        /// <value>
        /// The type of the book.
        /// </value>
        required public string BookType { get; set; }

        /// <summary>
        /// Gets or sets the book identifier.
        /// </summary>
        /// <value>
        /// The book identifier.
        /// </value>
        public Guid BookId { get; set; }

        /// <summary>
        /// Gets or sets the book.
        /// </summary>
        /// <value>
        /// The book.
        /// </value>
        public virtual Book? Book { get; set; }

        /// <summary>
        /// Gets or sets the book copies.
        /// </summary>
        /// <value>
        /// The book copies.
        /// </value>
        public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}
