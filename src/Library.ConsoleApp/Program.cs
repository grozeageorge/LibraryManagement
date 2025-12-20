// <copyright file="Program.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.ConsoleApp
{
    using System;
    using System.IO;
    using FluentValidation;
    using Library.ConsoleApp.Configuration;
    using Library.Data;
    using Library.Data.Repositories;
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Implementations;
    using Library.Services.Interfaces;
    using Library.Services.Validators;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        private static void Main(string[] args)
        {
            // 1. Setup Configuration
            ConfigurationBuilder? builder = new ConfigurationBuilder();
            BuildConfig(builder);
            IConfigurationRoot? configuration = builder.Build();

            // 2. Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Application Starting Up...");

                // 3. Create Host
                IHost? host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // Database Context
                        services.AddDbContext<LibraryDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("LibraryConnection")));

                        // Repositories
                        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                        // Configuration
                        services.AddSingleton<ILibraryConfiguration, JsonLibraryConfiguration>();

                        // Validators
                        services.AddScoped<IValidator<Book>, BookValidator>();
                        services.AddScoped<IValidator<Reader>, ReaderValidator>();

                        // Services
                        services.AddScoped<IBookService, BookService>();
                        services.AddScoped<IReaderService, ReaderService>();
                        services.AddScoped<ILendingService, LendingService>();
                    })
                    .UseSerilog() // Use Serilog instead of default .NET logger
                    .Build();

                // 4. Run Migration (Create DB automatically)
                using (IServiceScope? scope = host.Services.CreateScope())
                {
                    LibraryDbContext? db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                    db.Database.Migrate(); // This creates the DB and Tables if they don't exist
                    Log.Information("Database migration completed successfully.");
                }

                // 5. Run Application Logic (Example)
                // In a real app, you might have a MenuService or similar here.
                Console.WriteLine("Library Management System is running. Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start correctly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}