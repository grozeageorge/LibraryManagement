// <copyright file="ReaderServiceTests.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Tests.Services.Implementations
{
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using Library.Domain.Entities;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Tests.Helpers;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Tests for the reader service class to assure the good communication of the repository's behaviour and the database.
    /// </summary>
    [TestFixture]
    public class ReaderServiceTests
    {
        private Mock<IRepository<Reader>> mockRepository;
        private Mock<ILogger<ReaderService>> mockLogger;
        private Mock<IValidator<Reader>> mockValidator;
        private ReaderService service;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.mockRepository = new Mock<IRepository<Reader>>();
            this.mockLogger = new Mock<ILogger<ReaderService>>();
            this.mockValidator = new Mock<IValidator<Reader>>();
            this.service = new ReaderService(
                this.mockRepository.Object,
                this.mockLogger.Object,
                this.mockValidator.Object);
        }

        /// <summary>
        /// Register reader method should call repository to add and save changes in the database when the reader is valid.
        /// </summary>
        [Test]
        public void RegisterReader_ShouldCallRepository_WhenReaderIsValid()
        {
            // Arrange
            Reader reader = LibraryTestFactory.CreateReader();

            this.mockValidator.Setup(v => v.Validate(reader))
                .Returns(new ValidationResult());

            // Act
            this.service.RegisterReader(reader);

            // Assert
            this.mockRepository.VerifyAdd(Times.Once());
            this.mockRepository.VerifySaved();
        }

        /// <summary>
        /// Register reader method should throw an argument exception when the validation fails.
        /// </summary>
        [Test]
        public void RegisterReader_ShouldThrow_WhenValidationFails()
        {
            // Arrange
            Reader reader = new Reader
            {
                FirstName = string.Empty,
                LastName = "Doe",
                Address = "address",
                Email = "a@a.com",
            };

            var failure = new ValidationResult(new[]
            {
                new ValidationFailure("FirstName", "First name is required."),
            });

            this.mockValidator.Setup(v => v.Validate(reader))
                .Returns(failure);

            // Act
            Action act = () => this.service.RegisterReader(reader);

            // Assert
            act.Should().Throw<ArgumentException>();
            this.mockRepository.VerifyAdd(Times.Never());
        }
    }
}
