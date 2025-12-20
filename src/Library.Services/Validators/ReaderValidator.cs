// <copyright file="ReaderValidator.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Validators
{
    using FluentValidation;
    using Library.Domain.Entities;

    /// <summary>
    /// Validator for the Reader entity.
    /// Enforces constraints on personal data and contact info.
    /// </summary>
    public class ReaderValidator : AbstractValidator<Reader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderValidator"/> class.
        /// </summary>
        public ReaderValidator()
        {
            this.RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");

            this.RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

            this.RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            this.RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("A valid email address is required.");

            this.RuleFor(x => x)
                .Must(HaveAtLeastOneContactMethod)
                .WithMessage("At least one contact method (email or phone number) must be provided.");
        }

        /// <summary>
        /// Checks if the reader has at least one contact method.
        /// </summary>
        /// <param name="reader">The reader entity to validate.</param>
        /// <returns>
        /// <c>true</c> if at least one contact method is provided; otherwise, <c>false</c>.
        /// </returns>
        private static bool HaveAtLeastOneContactMethod(Reader reader)
        {
            return !string.IsNullOrEmpty(reader.Email) || !string.IsNullOrEmpty(reader.PhoneNumber);
        }
    }
}
