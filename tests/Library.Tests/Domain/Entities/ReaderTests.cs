// <copyright file="ReaderTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Domain.Entities
{
    using FluentAssertions;
    using Library.Domain.Entities;

    /// <summary>
    /// Tests for the Reader entity, mainly targeting the isLibrarian method.
    /// </summary>
    [TestFixture]
    public class ReaderTests
    {
        /// <summary>
        /// Determines if the method IsLibrarian returns true when the reader type is a librarian reader.
        /// </summary>
        [Test]
        public void IsLibrarian_ShouldReturnTrue_WhenTypeIsLibrarian()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                Email = "johndoe@example.com",
                PhoneNumber = "123-456-7890",
                Type = ReaderType.Librarian,
            };

            // Assert
            reader.IsLibrarian.Should().BeTrue();
        }

        /// <summary>
        /// Determines if the method IsLibrarian returns false if the reader type is a standard reader.
        /// </summary>
        [Test]
        public void IsLibrarian_ShouldReturnFalse_WhenTypeIsStandard()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                Email = "johndoe@example.com",
                PhoneNumber = "123-456-7890",
                Type = ReaderType.Standard,
            };

            // Assert
            reader.IsLibrarian.Should().BeFalse();
        }
    }
}
