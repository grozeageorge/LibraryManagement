// <copyright file="LibraryDbContext.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Data
{
    using Library.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The Entity Framework context for the Library application.
    /// Manages the connection to the database and the mapping of entities.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class LibraryDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the context.</param>
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the books table.
        /// </summary>
        /// <value>
        /// The books.
        /// </value>
        public DbSet<Book> Books { get; set; }

        /// <summary>
        /// Gets or sets the authors table.
        /// </summary>
        /// <value>
        /// The authors.
        /// </value>
        public DbSet<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the domains table.
        /// </summary>
        /// <value>
        /// The book domains.
        /// </value>
        public DbSet<BookDomain> Domains { get; set; }

        /// <summary>
        /// Gets or sets the readers table.
        /// </summary>
        /// <value>
        /// The readers.
        /// </value>
        public DbSet<Reader> Readers { get; set; }

        /// <summary>
        /// Gets or sets the loans table.
        /// </summary>
        /// <value>
        /// The loans.
        /// </value>
        public DbSet<Loan> Loans { get; set; }

        /// <summary>
        /// Gets or sets the book editions table.
        /// </summary>
        /// <value>
        /// The book editions.
        /// </value>
        public DbSet<BookEdition> BookEditions { get; set; }

        /// <summary>
        /// Gets or sets the book copies table.
        /// </summary>
        /// <value>
        /// The book copies.
        /// </value>
        public DbSet<BookCopy> BookCopies { get; set; }

        /// <summary>
        /// Configures the schema and relationships using Fluent API.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .UsingEntity(j => j.ToTable("BookAuthors"));

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Domains)
                .WithMany(d => d.Books)
                .UsingEntity(j => j.ToTable("BookBookDomains"));

            modelBuilder.Entity<BookDomain>()
                .HasOne(d => d.ParentDomain)
                .WithMany(d => d.SubDomains)
                .HasForeignKey(d => d.ParentDomainId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a parent if it has children (cascading)

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Editions)
                .WithOne(e => e.Book)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookEdition>()
                .HasMany(e => e.BookCopies)
                .WithOne(c => c.BookEdition)
                .HasForeignKey(c => c.BookEditionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Reader)
                .WithMany(r => r.Loans)
                .HasForeignKey(l => l.ReaderId);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.BookCopy)
                .WithMany() // BookCopy doesn't explicityle list its loans in the entity
                .HasForeignKey(l => l.BookCopyId);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Reader)
                .WithMany(r => r.Loans) // Loans borrowed by the reader
                .HasForeignKey(l => l.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Librarian)
                .WithMany(r => r.ProcessedLoans)
                .HasForeignKey(l => l.LibrarianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookCopy>()
                .Property(c => c.RowVersion)
                .IsRowVersion();
        }
    }
}
