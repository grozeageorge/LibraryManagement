// <copyright file="EntityEdgeCaseTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Domain.Entities
{
    using FluentAssertions;
    using Library.Domain.Entities;

    /// <summary>
    /// Edge Cases Tests for the entities of the library.
    /// </summary>
    [TestFixture]
    public class EntityEdgeCaseTests
    {
        /// <summary>
        /// Checks if an Author's Name is correctly formated when FullName method is called.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        /// <param name="expected">The expected.</param>
        [TestCase("John", "Doe", "John Doe")]
        [TestCase("   John   ", "Doe", "   John    Doe")]
        [TestCase("A", "B", "A B")]
        public void Author_FullName_ReturnsCorrectFormat(string first, string last, string expected)
        {
            Author author = new Author { FirstName = first, LastName = last };
            author.FullName.Should().Be(expected);
        }

        /// <summary>
        /// Tests if the NumberOfPages value can be change and can store an int.
        /// </summary>
        /// <param name="pages">The pages.</param>
        [TestCase(100)]
        [TestCase(1)]
        [TestCase(10000)]
        public void BookEdition_Pages_ShouldStoreValue(int pages)
        {
            BookEdition edition = new BookEdition { NumberOfPages = pages, Publisher = "P", BookType = "T" };
            edition.NumberOfPages.Should().Be(pages);
        }

        /// <summary>
        /// The default IsAvailable value for a new BookCopy should be true.
        /// </summary>
        [Test]
        public void BookCopy_DefaultStatus_ShouldBeAvailable()
        {
            BookCopy copy = new BookCopy();
            copy.IsAvailable.Should().BeTrue();
        }

        /// <summary>
        /// Checks if the default value for IsReadingRoomOnly for a BookCopy entity is false.
        /// </summary>
        [Test]
        public void BookCopy_DefaultReadingRoom_ShouldBeFalse()
        {
            BookCopy copy = new BookCopy();
            copy.IsReadingRoomOnly.Should().BeFalse();
        }

        /// <summary>
        /// Checks if the IsReadingRoomOnly attribute for BookCopy can be toggled.
        /// </summary>
        /// <param name="status">if set to <c>true</c> [status].</param>
        [TestCase(true)]
        [TestCase(false)]
        public void BookCopy_IsReadingRoomOnly_CanBeToggled(bool status)
        {
            BookCopy copy = new BookCopy { IsReadingRoomOnly = status };
            copy.IsReadingRoomOnly.Should().Be(status);
        }

        /// <summary>
        /// Checks if a new Loan has the extension days set at 0.
        /// </summary>
        [Test]
        public void Loan_ExtensionDays_ShouldStartAtZero()
        {
            Loan loan = new Loan();
            loan.ExtensionDaysCount.Should().Be(0);
        }

        /// <summary>
        /// Checks if setting the extension days count of a loan is correctly set.
        /// </summary>
        /// <param name="days">The days.</param>
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        public void Loan_ExtensionDays_CanBeSet(int days)
        {
            Loan loan = new Loan { ExtensionDaysCount = days };
            loan.ExtensionDaysCount.Should().Be(days);
        }

        /// <summary>
        /// Checks if the return date of a new loan object is null.
        /// </summary>
        [Test]
        public void Loan_ReturnDate_ShouldBeNullInitially()
        {
            Loan loan = new Loan();
            loan.ReturnDate.Should().BeNull();
        }

        /// <summary>
        /// Tests if the IsLibrarian method for reader is returning the correct value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="expected">if set to <c>true</c> [expected].</param>
        [TestCase(ReaderType.Standard, false)]
        [TestCase(ReaderType.Librarian, true)]
        public void Reader_IsLibrarian_ReturnsCorrectValue(ReaderType type, bool expected)
        {
            Reader reader = new Reader
            {
                Type = type,
                FirstName = "A",
                LastName = "B",
                Address = "C",
                Email = "D",
                PhoneNumber = "E",
            };

            reader.IsLibrarian.Should().Be(expected);
        }

        /// <summary>
        /// Determines if IsAncestorOf returns false if the ancestor domain it's the same.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldReturnFalse_WhenSelf()
        {
            BookDomain d1 = new BookDomain { Name = "A" };
            d1.IsAncestorOf(d1).Should().BeFalse();
        }

        /// <summary>
        /// Determines if IsAncestorOf works with a deep hierarchy of book domains.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldWork_WithDeepHierarchy()
        {
            BookDomain root = new BookDomain { Name = "Root" };
            BookDomain l1 = new BookDomain { Name = "L1", ParentDomain = root, ParentDomainId = root.Id };
            BookDomain l2 = new BookDomain { Name = "L2", ParentDomain = l1, ParentDomainId = l1.Id };
            BookDomain l3 = new BookDomain { Name = "L3", ParentDomain = l2, ParentDomainId = l2.Id };

            root.IsAncestorOf(l3).Should().BeTrue();
            l1.IsAncestorOf(l3).Should().BeTrue();
            l2.IsAncestorOf(l3).Should().BeTrue();
            l3.IsAncestorOf(root).Should().BeFalse();
        }
    }
}
