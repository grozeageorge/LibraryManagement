// <copyright file="BookValidatorTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Validators
{
    using FluentAssertions;
    using FluentValidation.Results;
    using Library.Domain.Entities;
    using Library.Services.Validators;
    using Library.Tests.Helpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests for the validator of the Book entity to check it's integrity.
    /// </summary>
    [TestFixture]
    public class BookValidatorTests
    {
        private BookValidator validator;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.validator = new BookValidator();
        }

        /// <summary>
        /// The validator should pass when a book is valid.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenBookIsValid()
        {
            // Arrange
            // Factory creates a valid book with Title, 1 Author, and 1 Domain by default
            Book book = LibraryTestFactory.CreateBook();
            book.Authors.Add(new Author { FirstName = "John", LastName = "Doe" });

            // Act
            ValidationResult result = this.validator.Validate(book);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        /// <summary>
        /// The validator should fail when the title is missing.
        /// </summary>
        /// <param name="invalidTitle">The invalid title.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Validate_ShouldFail_WhenTitleIsMissing(string? invalidTitle)
        {
            // Arrange
            // We use '!' (null-forgiving operator) to force null into the required property
            // because we specifically want to test that the Validator catches this error.
            Book book = new Book
            {
                Title = invalidTitle!,

                // Add valid author/domain so we only fail on Title
                Authors = { new Author { FirstName = "A", LastName = "B" } },
                Domains = { new BookDomain { Name = "D" } },
            };

            // Act
            ValidationResult result = this.validator.Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Title");
        }

        /// <summary>
        /// The validator should fail when the title is too long.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenTitleIsTooLong()
        {
            // Arrange
            Book book = new Book
            {
                Title = new string('a', 201), // Limit is 200
                Authors = { new Author { FirstName = "A", LastName = "B" } },
                Domains = { new BookDomain { Name = "D" } },
            };

            // Act
            ValidationResult result = this.validator.Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Title");
        }

        /// <summary>
        /// The validator should fail when the list of authors is empty.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenAuthorsListIsEmpty()
        {
            // Arrange
            Book book = new Book
            {
                Title = "Valid Title",
                Domains = { new BookDomain { Name = "D" } },

                // Authors is empty by default
            };

            // Act
            ValidationResult result = this.validator.Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Authors");
        }

        /// <summary>
        /// The validator should fail when the list of domains is empty.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenDomainsListIsEmpty()
        {
            // Arrange
            Book book = new Book
            {
                Title = "Valid Title",
                Authors = { new Author { FirstName = "A", LastName = "B" } },

                // Domains is empty by default in manual creation,
                // but let's ensure it's empty just in case
            };
            book.Domains.Clear();

            // Act
            ValidationResult result = this.validator.Validate(book);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Domains");
        }
    }
}