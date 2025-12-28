// <copyright file="BookTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Domain.Entities
{
    using FluentAssertions;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Moq;

    /// <summary>
    /// Tests for the book entity, mainly targeting the AddDomain method.
    /// </summary>
    [TestFixture]
    public class BookTests
    {
        private Mock<ILibraryConfiguration> mockConfig;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockConfig = new Mock<ILibraryConfiguration>();
        }

        /// <summary>
        /// Add domain should add a domain when valid.
        /// </summary>
        [Test]
        public void AddDomain_ShouldAddDomain_WhenValid()
        {
            // Arrange
            Book book = new Book { Title = "Test Book" };
            BookDomain domain = new BookDomain { Name = "Science" };

            this.mockConfig.Setup(c => c.MaxDomainsPerBook).Returns(3);

            // Act
            book.AddDomain(domain, this.mockConfig.Object);

            // Assert
            book.Domains.Should().Contain(domain);
            book.Domains.Count.Should().Be(1);
        }

        /// <summary>
        /// Adding a domain should throw exception when domain is null.
        /// </summary>
        [Test]
        public void AddDomain_ShouldThrowException_WhenDomainIsNull()
        {
            // Arrange
            Book book = new Book { Title = "Test Book" };

            this.mockConfig.Setup(c => c.MaxDomainsPerBook).Returns(3);

            // Act
            Action act = () => book.AddDomain(null, this.mockConfig.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Adding a domain should throw exception when maximum domains exceeded.
        /// </summary>
        [Test]
        public void AddDomain_ShouldThrowException_WhenMaxDomainsExceeded()
        {
            // Arrange
            Book book = new Book { Title = "Test Book" };

            this.mockConfig.Setup(c => c.MaxDomainsPerBook).Returns(2);

            book.AddDomain(new BookDomain { Name = "Science" }, this.mockConfig.Object);
            book.AddDomain(new BookDomain { Name = "Arts" }, this.mockConfig.Object);

            BookDomain newDomain = new BookDomain { Name = "Extra Domain" };

            // Act
            Action act = () => book.AddDomain(newDomain, this.mockConfig.Object);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        /// <summary>
        /// Adding an already existing domain should not add it again.
        /// </summary>
        [Test]
        public void AddDomain_ShouldIgnore_WhenDomainAlreadyExists()
        {
            // Arrange
            Book book = new Book { Title = "Test Book" };
            BookDomain domain = new BookDomain { Name = "Science" };

            this.mockConfig.Setup(c => c.MaxDomainsPerBook).Returns(3);

            book.AddDomain(domain, this.mockConfig.Object);

            // Act
            book.AddDomain(domain, this.mockConfig.Object);

            // Assert
            book.Domains.Count.Should().Be(1);
        }

        /// <summary>
        /// Adding a domain should throw exception when adding an ancestor of an existing domain.
        /// </summary>
        [Test]
        public void AddDomain_ShouldThrowException_WhenAddingAncestorOfExistingDomain()
        {
            // Arrange
            BookDomain parent = new BookDomain { Name = "Science" };
            BookDomain child = new BookDomain { Name = "Physics", ParentDomain = parent, ParentDomainId = parent.Id };

            Book book = new Book { Title = "Test Book" };

            this.mockConfig.Setup(c => c.MaxDomainsPerBook).Returns(3);

            book.AddDomain(child, this.mockConfig.Object); // Add Child First

            // Act
            Action act = () => book.AddDomain(parent, this.mockConfig.Object);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*ancestor*");
        }

        /// <summary>
        /// Adding a domain should throw exception when adding a descendant of an existing domain.
        /// </summary>
        [Test]
        public void AddDomain_ShouldThrowException_WhenAddingDescendantOfExistingDomain()
        {
            // Arrange
            BookDomain parent = new BookDomain { Name = "Science" };
            BookDomain child = new BookDomain { Name = "Physics", ParentDomain = parent, ParentDomainId = parent.Id };

            Book book = new Book { Title = "Test Book" };

            this.mockConfig.Setup(c => c.MaxDomainsPerBook).Returns(3);

            book.AddDomain(parent, this.mockConfig.Object); // Add Parent First

            // Act
            Action act = () => book.AddDomain(child, this.mockConfig.Object);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*ancestor*");
        }
    }
}
