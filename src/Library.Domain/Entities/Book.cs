// <copyright file="Book.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    using Library.Domain.Interfaces;

    /// <summary>
    /// The book entity that represents a book in the library system.
    /// Manages authors, domains and editions.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class Book : BaseEntity
    {
        /// <summary>
        /// Gets or sets the title of the book.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        required public string Title { get; set; }

        /// <summary>
        /// Gets or sets the authors of the book.
        /// </summary>
        /// <value>
        /// The authors.
        /// </value>
        public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

        /// <summary>
        /// Gets or sets the domains that the book is part of.
        /// </summary>
        /// <value>
        /// The domains.
        /// </value>
        public virtual ICollection<BookDomain> Domains { get; set; } = new List<BookDomain>();

        /// <summary>
        /// Gets or sets the editions of the book.
        /// </summary>
        /// <value>
        /// The editions of the book.
        /// </value>
        public virtual ICollection<BookEdition> Editions { get; set; } = new List<BookEdition>();

        /// <summary>
        /// Adds a new domain.
        /// </summary>
        /// <param name="domain">The domain to be added.</param>
        /// <param name="config">The maximum domains allowed.</param>
        public void AddDomain(BookDomain? domain, ILibraryConfiguration config)
        {
            if (domain == null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            int maxDomainsAllowed = config.MaxDomainsPerBook;

            if (this.Domains.Count >= maxDomainsAllowed)
            {
                throw new InvalidOperationException($"A book cannot have more than {maxDomainsAllowed} domains.");
            }

            foreach (BookDomain existingDomain in this.Domains)
            {
                if (existingDomain.Id == domain.Id)
                {
                    return; // Domain already exists, no need to add
                }

                if (domain.IsAncestorOf(existingDomain))
                {
                    throw new InvalidOperationException($"Cannot add domain '{domain.Name}' because it is an ancestor of an existing domain '{existingDomain.Name}'.");
                }

                if (existingDomain.IsAncestorOf(domain))
                {
                    throw new InvalidOperationException($"Cannot add domain '{domain.Name}' because it's ancestor '{existingDomain.Name}' is already explicitly defined.");
                }
            }

            this.Domains.Add(domain);
        }
    }
}
