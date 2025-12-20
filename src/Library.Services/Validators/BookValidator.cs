// <copyright file="BookValidator.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Validators
{
    using FluentValidation;
    using Library.Domain.Entities;

    /// <summary>
    /// Validator for the Book entity.
    /// Enforces constrains on title, authors, and domains.
    /// </summary>
    public class BookValidator : AbstractValidator<Book>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BookValidator"/> class.
        /// </summary>
        public BookValidator()
        {
            this.RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Book title is required.")
                .MaximumLength(200).WithMessage("Book title cannot exceed 200 characters.");

            this.RuleFor(x => x.Authors)
                .NotEmpty().WithMessage("A book must have at least one author.");

            this.RuleFor(x => x.Domains)
                .NotEmpty().WithMessage("A book must belong to at least one domain.");
        }
    }
}
