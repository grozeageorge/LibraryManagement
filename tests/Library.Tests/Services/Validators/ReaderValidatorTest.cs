// <copyright file="ReaderValidatorTest.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Validators
{
    using FluentAssertions;
    using FluentValidation.Results;
    using Library.Domain.Entities;
    using Library.Services.Validators;

    /// <summary>
    /// Reader validator tests to verify that the validation rules are correctly enforced.
    /// </summary>
    [TestFixture]
    public class ReaderValidatorTest
    {
        private ReaderValidator validator;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.validator = new ReaderValidator();
        }

        /// <summary>
        /// The reader validator should pass when a reader is valid.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenReaderIsValid()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Street",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
            };

            // Act
            ValidationResult result = this.validator.Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        /// <summary>
        /// The reader validator should fail when reader's first name is empty.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenFirstNameIsEmpty()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = string.Empty,
                LastName = "Doe",
                Address = "123 Street",
                Email = "john@example.com",
                PhoneNumber = "123",
            };

            // Act
            ValidationResult result = this.validator.Validate(reader);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        }

        /// <summary>
        /// The reader validator should fail when the reader has no contact method provided.
        /// </summary>
        [Test]
        public void Validate_ShouldFail_WhenNoContactMethodProvided()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Street",
                Email = string.Empty,
                PhoneNumber = string.Empty,
            };

            // Act
            ValidationResult result = this.validator.Validate(reader);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("At least one contact method"));
        }

        /// <summary>
        /// The reader validator should pass when the reader has provided only email.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenOnlyEmailProvided()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Street",
                Email = "john@example.com",
                PhoneNumber = string.Empty,
            };

            // Act
            ValidationResult result = this.validator.Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        /// <summary>
        /// The reader validator should pass when the reader has provided only the phone number.
        /// </summary>
        [Test]
        public void Validate_ShouldPass_WhenOnlyPhoneNumberProvided()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Street",
                Email = string.Empty,
                PhoneNumber = "1234567890",
            };

            // Act
            ValidationResult result = this.validator.Validate(reader);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
