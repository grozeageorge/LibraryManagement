// <copyright file="LibraryDbContextFactory.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.ConsoleApp
{
    using Library.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    /// <summary>
    /// Defines the factory for creating the <see cref="LibraryDbContext"/> at design time.
    /// This class is used exclusively by Entity Framework Core tools (e.g., Add-Migration)
    /// to instantiate the context without running the application entry point.
    /// </summary>
    public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LibraryDbContext"/>.
        /// </summary>
        /// <param name="args">Arguments provided by the design-time tool.</param>
        /// <returns>A configured instance of <see cref="LibraryDbContext"/>.</returns>
        public LibraryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();

            optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=LibraryDb;Integrated Security=True;TrustServerCertificate=True;");

            return new LibraryDbContext(optionsBuilder.Options);
        }
    }
}