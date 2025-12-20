// <copyright file="Author.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Represents an author of books in the library system.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class Author : BaseEntity
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        required public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        required public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the collection of books that were written by this author.
        /// </summary>
        /// <value>
        /// The books.
        /// </value>
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName => $"{this.FirstName} {this.LastName}";
    }
}
