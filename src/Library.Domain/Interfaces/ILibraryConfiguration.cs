// <copyright file="ILibraryConfiguration.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Interfaces
{
    /// <summary>
    /// Defines the configurable limits and constraints for the library system.
    /// </summary>
    public interface ILibraryConfiguration
    {
        /// <summary>
        /// Gets the maximum number of domains a book can belong to (DOMENII).
        /// </summary>
        int MaxDomainsPerBook { get; }

        /// <summary>
        /// Gets the maximum number of books a reader can borrow (NMC).
        /// </summary>
        int MaxBooksPerReader { get; }

        /// <summary>
        /// Gets the loan limit period in months (PER).
        /// </summary>
        int LoanLimitPeriodMonths { get; }

        /// <summary>
        /// Gets the maximum books per loan (C).
        /// </summary>
        int MaxBooksPerLoan { get; }

        /// <summary>
        /// Gets the maximum books per domain in history (D).
        /// </summary>
        int MaxBooksPerDomain { get; }

        /// <summary>
        /// Gets the domain check interval in months (L).
        /// </summary>
        int DomainCheckIntervalMonths { get; }

        /// <summary>
        /// Gets the standard loan period in days.
        /// </summary>
        int LoanPeriodDays { get; }

        /// <summary>
        /// Gets the max extension days (LIM).
        /// </summary>
        int MaxExtensionDays { get; }

        /// <summary>
        /// Gets the re-borrow restricted days (DELTA).
        /// </summary>
        int ReborrowRestrictedDays { get; }

        /// <summary>
        /// Gets the max books per day (NCZ).
        /// </summary>
        int MaxBooksPerDay { get; }

        /// <summary>
        /// Gets the max processed books for librarian (PERSIMP).
        /// </summary>
        int MaxProcessedPerDayLibrarian { get; }
    }
}