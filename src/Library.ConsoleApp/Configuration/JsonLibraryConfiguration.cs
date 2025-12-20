// <copyright file="JsonLibraryConfiguration.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.ConsoleApp.Configuration
{
    using Library.Domain.Interfaces;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Reads library settings from appsettings.json .
    /// </summary>
    /// <seealso cref="Library.Domain.Interfaces.ILibraryConfiguration" />
    public class JsonLibraryConfiguration : ILibraryConfiguration
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLibraryConfiguration"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public JsonLibraryConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the maximum number of domains a book can belong to (DOMENII).
        /// </summary>
        public int MaxDomainsPerBook => this.GetValue("MaxDomainsPerBook", 3);

        /// <summary>
        /// Gets the maximum number of books a reader can borrow (NMC).
        /// </summary>
        public int MaxBooksPerReader => this.GetValue("MaxBooksPerReader", 5);

        /// <summary>
        /// Gets the loan limit period in months (PER).
        /// </summary>
        public int LoanLimitPeriodMonths => this.GetValue("LoanLimitPeriodMonths", 6);

        /// <summary>
        /// Gets the maximum books per loan (C).
        /// </summary>
        public int MaxBooksPerLoan => this.GetValue("MaxBooksPerLoan", 3);

        /// <summary>
        /// Gets the maximum books per domain in history (D).
        /// </summary>
        public int MaxBooksPerDomain => this.GetValue("MaxBooksPerDomain", 3);

        /// <summary>
        /// Gets the domain check interval in months (L).
        /// </summary>
        public int DomainCheckIntervalMonths => this.GetValue("DomainCheckIntervalMonths", 3);

        /// <summary>
        /// Gets the standard loan period in days.
        /// </summary>
        public int LoanPeriodDays => this.GetValue("LoanPeriodDays", 14);

        /// <summary>
        /// Gets the max extension days (LIM).
        /// </summary>
        public int MaxExtensionDays => this.GetValue("MaxExtensionDays", 30);

        /// <summary>
        /// Gets the re-borrow restricted days (DELTA).
        /// </summary>
        public int ReborrowRestrictedDays => this.GetValue("ReborrowRestrictedDays", 90);

        /// <summary>
        /// Gets the max books per day (NCZ).
        /// </summary>
        public int MaxBooksPerDay => this.GetValue("MaxBooksPerDay", 2);

        /// <summary>
        /// Gets the max processed books for librarian (PERSIMP).
        /// </summary>
        public int MaxProcessedPerDayLibrarian => this.GetValue("MaxProcessedPerDayLibrarian", 20);

        private int GetValue(string key, int defaultValue)
        {
            return this.configuration.GetValue<int>($"LibrarySettings:{key}", defaultValue);
        }
    }
}
