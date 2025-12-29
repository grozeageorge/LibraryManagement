// <copyright file="MockExtensions.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Helpers
{
    using System.Linq.Expressions;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Moq;

    /// <summary>
    /// Configures the mock of repositories for testing purposes.
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Setups the configuration default limits.
        /// </summary>
        /// <param name="mock">The mock.</param>
        /// <param name="maxBooksPerReader">The maximum books per reader.</param>
        /// <param name="maxBooksPerDay">The maximum books per day.</param>
        /// <param name="maxBooksPerLoan">The maximum books per loan.</param>
        /// <param name="maxBooksPerDomain">The maximum books per domain.</param>
        /// <param name="maxExtensionDays">The maximum extension days.</param>
        /// <param name="maxProcessedPerDayLibrarian">The maximum processed per day librarian.</param>
        /// <param name="domainCheckIntervalMonths">The domain check interval months.</param>
        /// <param name="loanPeriodDays">The loan period days.</param>
        /// <param name="reborrowRestrictedDays">The reborrow restricted days.</param>
        /// <param name="maxDomainsPerBook">The maximum domains per book.</param>
        public static void SetupConfigDefaultLimits(
            this Mock<ILibraryConfiguration> mock,
            int maxBooksPerReader = 100,
            int maxBooksPerDay = 100,
            int maxBooksPerLoan = 100,
            int maxBooksPerDomain = 100,
            int maxExtensionDays = 100,
            int maxProcessedPerDayLibrarian = 100,
            int domainCheckIntervalMonths = 1,
            int loanPeriodDays = 14,
            int reborrowRestrictedDays = 0,
            int maxDomainsPerBook = 10)
        {
            mock.Setup(c => c.MaxBooksPerReader).Returns(maxBooksPerReader);
            mock.Setup(c => c.MaxBooksPerDay).Returns(maxBooksPerDay);
            mock.Setup(c => c.MaxBooksPerLoan).Returns(maxBooksPerLoan);
            mock.Setup(c => c.MaxBooksPerDomain).Returns(maxBooksPerDomain);
            mock.Setup(c => c.MaxExtensionDays).Returns(maxExtensionDays);
            mock.Setup(c => c.MaxProcessedPerDayLibrarian).Returns(maxProcessedPerDayLibrarian);
            mock.Setup(c => c.DomainCheckIntervalMonths).Returns(domainCheckIntervalMonths);
            mock.Setup(c => c.LoanPeriodDays).Returns(loanPeriodDays);
            mock.Setup(c => c.ReborrowRestrictedDays).Returns(reborrowRestrictedDays);
            mock.Setup(c => c.MaxDomainsPerBook).Returns(maxDomainsPerBook);
        }

        /// <summary>
        /// Setups the mocked repository to get by id what is needed.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mock">The mock of the repository.</param>
        /// <param name="id">The identifier to pass in the GetById method.</param>
        /// <param name="entity">The entity that is found by the repository.</param>
        public static void SetupGetById<T>(this Mock<IRepository<T>> mock, Guid id, T? entity)
            where T : BaseEntity
        {
            mock.Setup(r => r.GetById(id)).Returns(entity);
        }

        /// <summary>
        /// Setups the mocked repository to find what is needed.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mock">The mock of the repository.</param>
        /// <param name="results">The results that are found by the repository.</param>
        public static void SetupFind<T>(this Mock<IRepository<T>> mock, IEnumerable<T> results)
            where T : BaseEntity
        {
            mock.Setup(r => r.Find(It.IsAny<Expression<Func<T, bool>>>())).Returns(results);
        }

        /// <summary>
        /// Verifies if the mocked repository is adding.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mock">The mock of the repository.</param>
        /// <param name="times">The times the repository adds..</param>
        public static void VerifyAdd<T>(this Mock<IRepository<T>> mock, Times times)
            where T : BaseEntity
        {
            mock.Verify(r => r.Add(It.IsAny<T>()), times);
        }

        /// <summary>
        /// Verifies if the mocked repository saved.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mock">The mock of the repository.</param>
        public static void VerifySaved<T>(this Mock<IRepository<T>> mock)
            where T : BaseEntity
        {
            mock.Verify(r => r.SaveChanges(), Times.Once);
        }
    }
}
