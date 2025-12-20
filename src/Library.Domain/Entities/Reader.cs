// <copyright file="Reader.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Reader entity that represents a reader or a staff member in the library system.
    /// </summary>
    /// <seealso cref="Library.Domain.Entities.BaseEntity" />
    public class Reader : BaseEntity
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
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        required public string Address { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        required public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        required public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of the reader.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public ReaderType Type { get; set; } = ReaderType.Standard;

        /// <summary>
        /// Gets or sets the history of loans associated with this reader.
        /// </summary>
        /// <value>
        /// The loans.
        /// </value>
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

        /// <summary>
        /// Gets a value indicating whether determines whether this instance has librarian priviledges.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is librarian; otherwise, <c>false</c>.
        /// </returns>
        public bool IsLibrarian => this.Type == ReaderType.Librarian;
    }
}
