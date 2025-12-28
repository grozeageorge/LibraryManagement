// <copyright file="AuthorTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Domain.Entities
{
    using FluentAssertions;
    using Library.Domain.Entities;

    /// <summary>
    /// Tests for the Author entity that targets basic requirements.
    /// </summary>
    [TestFixture]
    public class AuthorTests
    {
        /// <summary>
        /// Full name method should return the first name and the last name.
        /// </summary>
        [Test]
        public void FullName_ShouldReturnFirstAndLastName()
        {
            // Arrange
            Author author = new Author { FirstName = "John", LastName = "Doe" };

            // Act
            string result = author.FullName;

            // Assert
            result.Should().Be("John Doe");
        }

        /// <summary>
        /// The constructor should generate a new identifier.
        /// </summary>
        [Test]
        public void Constructor_ShouldGenerateNewId()
        {
            // Arrange
            Author author = new Author { FirstName = "John", LastName = "Doe" };

            // Assert
            author.Id.Should().NotBeEmpty();
        }
    }
}
