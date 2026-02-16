// <copyright file="BookDomain.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Represents a category or domain for books (e.g., Science, Computer Science).
    /// Supports hierarchical relationships.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class BookDomain : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name of the domain.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        required public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent domain identifier.
        /// </summary>
        /// <value>
        /// The parent domain identifier.
        /// </value>
        public Guid? ParentDomainId { get; set; }

        /// <summary>
        /// Gets or sets the parent domain.
        /// </summary>
        /// <value>
        /// The parent domain.
        /// </value>
        public virtual BookDomain? ParentDomain { get; set; }

        /// <summary>
        /// Gets or sets the collection of sub domains.
        /// </summary>
        /// <value>
        /// The sub domains.
        /// </value>
        public virtual ICollection<BookDomain> SubDomains { get; set; } = new List<BookDomain>();

        /// <summary>
        /// Gets or sets the collection of books associated to this domain.
        /// </summary>
        /// <value>
        /// The books.
        /// </value>
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        /// <summary>
        /// Determines whether is ancestor of the specified domain.
        /// </summary>
        /// <param name="domain">The domain to check against.</param>
        /// <returns>
        ///   <c>true</c> if is ancestor of the specified domain; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAncestorOf(BookDomain? domain)
        {
            if (domain == null)
            {
                return false;
            }

            BookDomain? current = domain.ParentDomain;
            while (current != null)
            {
                if (current.Id == this.Id)
                {
                    return true;
                }

                current = current.ParentDomain;
            }

            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => this.Name;
    }
}
