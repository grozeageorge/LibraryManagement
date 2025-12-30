// <copyright file="BookCopy.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Book copy entity that represents a physical copy of a book edition in the library system.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class BookCopy : BaseEntity
    {
        /// <summary>
        /// Gets or sets the book edition identifier.
        /// </summary>
        /// <value>
        /// The book edition identifier.
        /// </value>
        public Guid BookEditionId { get; set; }

        /// <summary>
        /// Gets or sets the book edition.
        /// </summary>
        /// <value>
        /// The book edition.
        /// </value>
        public virtual BookEdition? BookEdition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reading room only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is reading room only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadingRoomOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Gets or sets the row version which is a concurrency token. Automatically updated by the database on every modification. Optimistic concurrency.
        /// </summary>
        /// <value>
        /// The row version.
        /// </value>
        public byte[]? RowVersion { get; set; }
    }
}
