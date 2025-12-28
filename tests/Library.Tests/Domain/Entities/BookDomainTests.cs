// <copyright file="BookDomainTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Domain.Entities
{
    using FluentAssertions;
    using Library.Domain.Entities;

    /// <summary>
    /// Tests for the book domain entity, mainly for the method IsAncestorOf.
    /// </summary>
    [TestFixture]
    public class BookDomainTests
    {
        /// <summary>
        /// Determines if the method IsAncestorOf returns false when a potential descendant is null.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldReturnFalse_WhenPotentialDescendantIsNull()
        {
            // Arrange
            BookDomain parent = new BookDomain { Name = "Science" };

            // Act
            bool result = parent.IsAncestorOf(null);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Determines if the method IsAncestorOf returns true when domain is the direct parent.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldReturnTrue_WhenDomainIsDirectParent()
        {
            // Arrange
            BookDomain parent = new BookDomain { Name = "Science" };
            BookDomain child = new BookDomain { Name = "Physics", ParentDomain = parent, ParentDomainId = parent.Id };

            // Act
            bool result = parent.IsAncestorOf(child);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Determines if the method IsAncestorOf returns true when domain is a grand parent.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldReturnTrue_WhenDomainIsGrandParent()
        {
            // Arrange
            BookDomain grandParent = new BookDomain { Name = "Science" };
            BookDomain parent = new BookDomain { Name = "Physics", ParentDomain = grandParent, ParentDomainId = grandParent.Id };
            BookDomain child = new BookDomain { Name = "Quantum Mechanics", ParentDomain = parent, ParentDomainId = parent.Id };

            // Act
            bool result = grandParent.IsAncestorOf(child);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Determines if the method IsAncestorOf returns false when domains are sibilings.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldReturnFalse_WhenDomainsAreSibilings()
        {
            // Arrange
            BookDomain parent = new BookDomain { Name = "Science" };
            BookDomain child1 = new BookDomain { Name = "Physics", ParentDomain = parent, ParentDomainId = parent.Id };
            BookDomain child2 = new BookDomain { Name = "Chemistry", ParentDomain = parent, ParentDomainId = parent.Id };

            // Act
            bool result = child1.IsAncestorOf(child2);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Determines if the method IsAncestorOf returns false when domain is a child.
        /// </summary>
        [Test]
        public void IsAncestorOf_ShouldReturnFalse_WhenDomainIsChild()
        {
            // Arrange
            BookDomain parent = new BookDomain { Name = "Science" };
            BookDomain child = new BookDomain { Name = "Physics", ParentDomain = parent, ParentDomainId = parent.Id };

            // Act
            bool result = child.IsAncestorOf(parent);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Converting to a string should return the name of the book domain.
        /// </summary>
        [Test]
        public void ToString_ShouldReturnName()
        {
            // Arrange
            BookDomain domain = new BookDomain { Name = "History" };

            // Act
            string result = domain.ToString();

            // Assert
            result.Should().Be("History");
        }
    }
}
