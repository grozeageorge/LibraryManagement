// <copyright file="LendingService.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Services.Implementations
{
    using Library.Domain.Entities;
    using Library.Domain.Interfaces;
    using Library.Domain.Repositories;
    using Library.Services.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service that implements the complex rules and constraints.
    /// </summary>
    /// <seealso cref="Library.Services.Interfaces.ILendingService" />
    public class LendingService : ILendingService
    {
        private readonly IRepository<Loan> loanRepository;
        private readonly IRepository<Reader> readerRepository;
        private readonly IRepository<BookCopy> copyRepository;
        private readonly IRepository<Book> bookRepository;
        private readonly ILibraryConfiguration config;
        private readonly ILogger<LendingService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LendingService"/> class.
        /// </summary>
        /// <param name="loanRepository">The loan repository.</param>
        /// <param name="readerRepository">The reader repository.</param>
        /// <param name="copyRepository">The copy repository.</param>
        /// <param name="bookRepository">The book repository.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public LendingService(
            IRepository<Loan> loanRepository,
            IRepository<Reader> readerRepository,
            IRepository<BookCopy> copyRepository,
            IRepository<Book> bookRepository,
            ILibraryConfiguration config,
            ILogger<LendingService> logger)
        {
            this.loanRepository = loanRepository;
            this.readerRepository = readerRepository;
            this.copyRepository = copyRepository;
            this.bookRepository = bookRepository;
            this.config = config;
            this.logger = logger;
        }

        /// <summary>
        /// Borrows the book.
        /// </summary>
        /// <param name="readerId">The reader identifier.</param>
        /// <param name="bookCopyId">The book copy identifier.</param>
        /// <param name="librarianId">The librarian identifier.</param>
        /// <exception cref="ArgumentException">Reader not found.
        /// or
        /// Book copy not found.</exception>
        /// <exception cref="InvalidOperationException">Book hierarchy is incomplete (missing Edition or Book).
        /// or
        /// This book copy is already borrowed.
        /// or
        /// This copy is restricted to the reading room.</exception>
        public void BorrowBook(Guid readerId, Guid bookCopyId, Guid? librarianId = null)
        {
            this.logger.LogInformation($"Starting borrow process for reader {readerId} and book copy {bookCopyId}");

            Reader reader = this.readerRepository.GetById(readerId)
                ?? throw new ArgumentException("Reader not found.");

            BookCopy copy = this.copyRepository.GetById(bookCopyId)
                ?? throw new ArgumentException("Book copy not found.");

            BookEdition? edition = copy.BookEdition;
            Book? book = edition?.Book;

            if (book == null)
            {
                throw new InvalidOperationException("Book hierarchy is incomplete (missing Edition or Book).");
            }

            if (!copy.IsAvailable)
            {
                throw new InvalidOperationException("This book copy is already borrowed.");
            }

            if (copy.IsReadingRoomOnly)
            {
                throw new InvalidOperationException("This copy is restricted to the reading room.");
            }

            this.ValidateStockRule(book.Id); // Check stock rule (10%)

            this.ValidateReaderLimits(reader); // Check reader limits (NMC, NCZ, etc.)

            this.ValidateDomainHistory(reader, book); // Check domain history (D in L months)

            this.ValidateReborrowDelta(reader, copy); // Check re-borrow delta

            if (librarianId.HasValue)
            {
                this.ValidateLibrarianLimit(librarianId.Value);
            }

            Loan loan = new Loan
            {
                ReaderId = readerId,
                BookCopyId = bookCopyId,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(this.config.LoanPeriodDays),
                ExtensionDaysCount = 0,
            };

            copy.IsAvailable = false;

            try
            {
                this.loanRepository.Add(loan);
                this.loanRepository.SaveChanges();
                this.logger.LogInformation("Book borrowed successfully.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to process loan.");
                throw;
            }
        }

        /// <summary>
        /// Returns the book.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <exception cref="ArgumentException">Loan not found.</exception>
        /// <exception cref="InvalidOperationException">Book is already returned.</exception>
        public void ReturnBook(Guid loanId)
        {
            Loan loan = this.loanRepository.GetById(loanId)
                ?? throw new ArgumentException("Loan not found.");

            if (loan.ReturnDate != null)
            {
                throw new InvalidOperationException("Book is already returned.");
            }

            loan.ReturnDate = DateTime.Now;

            BookCopy? copy = this.copyRepository.GetById(loan.BookCopyId);
            if (copy != null)
            {
                copy.IsAvailable = true;
            }

            this.loanRepository.SaveChanges();
            this.logger.LogInformation("Book returned successfully.");
        }

        /// <summary>
        /// Extends the due date of an active loan.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="days">The number of days to extend.</param>
        /// <exception cref="ArgumentException">
        /// Extension days must be positive.
        /// or
        /// Loan not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Cannot extend a returned book.
        /// or
        /// Reader associated with loan not found.
        /// or
        /// Cannot extend. Total extension days would exceed the limit of {limit}. Current: {loan.ExtensionDaysCount}.
        /// </exception>
        public void ExtendLoan(Guid loanId, int days)
        {
            this.logger.LogInformation($"Attempting to extend loan {loanId} by {days} days.");

            if (days <= 0)
            {
                throw new ArgumentException("Extension days must be positive.");
            }

            Loan loan = this.loanRepository.GetById(loanId)
                        ?? throw new ArgumentException("Loan not found.");

            if (loan.ReturnDate != null)
            {
                throw new InvalidOperationException("Cannot extend a returned book.");
            }

            // Load Reader to check if librarian
            // If lazy loading is off, we might need to fetch reader explicitly.
            // We assume generic repo getbyid doesn't include navigation properties by default unless configured.
            // Fetching the reader to be safe.
            Reader? reader = this.readerRepository.GetById(loan.ReaderId);
            if (reader == null)
            {
                throw new InvalidOperationException("Reader associated with loan not found.");
            }

            int limit = this.config.MaxExtensionDays;
            if (reader.IsLibrarian)
            {
                limit *= 2;
            }

            // Check constraint, sum < limit
            if (loan.ExtensionDaysCount + days > limit)
            {
                throw new InvalidOperationException($"Cannot extend. Total extension days would exceed the limit of {limit}. Current: {loan.ExtensionDaysCount}.");
            }

            // Apply extension
            loan.DueDate = loan.DueDate.AddDays(days);
            loan.ExtensionDaysCount += days;

            this.loanRepository.SaveChanges();
            this.logger.LogInformation($"Loan extended successfully. New DueDate: {loan.DueDate}.");
        }

        /// <summary>
        /// Borrows multiple books in a single transaction. Enforces the "3 books must be from 2 categories" rule.
        /// </summary>
        /// <param name="readerId">The reader identifier.</param>
        /// <param name="bookCopyIds">List of book copy ids.</param>
        /// <exception cref="ArgumentException">
        /// Must select at least one book.
        /// or
        /// Reader not found.
        /// or
        /// Book copy {copyId} not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Cannot borrow more than {limit} books at once.
        /// or
        /// Book data incomplete.
        /// or
        /// When borrowing 3 or more books, they must come from at least 2 distinct categories.
        /// </exception>
        public void BorrowBooks(Guid readerId, IEnumerable<Guid> bookCopyIds)
        {
            List<Guid> copyIdList = bookCopyIds.ToList();

            if (!copyIdList.Any())
            {
                throw new ArgumentException("Must select at least one book.");
            }

            // Check Limit C (Max books per loan)
            Reader reader = this.readerRepository.GetById(readerId)
                            ?? throw new ArgumentException("Reader not found.");

            int limit = this.config.MaxBooksPerLoan;
            if (reader.IsLibrarian)
            {
                limit *= 2;
            }

            if (copyIdList.Count > limit)
            {
                throw new InvalidOperationException($"Cannot borrow more than {limit} books at once.");
            }

            // Check 3 books -> 2 categories rule
            if (copyIdList.Count >= 3)
            {
                HashSet<Guid> distinctDomainIds = new HashSet<Guid>();

                foreach (Guid copyId in copyIdList)
                {
                    BookCopy copy = this.copyRepository.GetById(copyId)
                                    ?? throw new ArgumentException($"Book copy {copyId} not found.");

                    // Ensure hierarchy is loaded
                    if (copy.BookEdition?.Book == null)
                    {
                        // Try to load book if missing (simplified in real app use .Include()
                        throw new InvalidOperationException("Book data incomplete.");
                    }

                    foreach (BookDomain domain in copy.BookEdition.Book.Domains)
                    {
                        distinctDomainIds.Add(domain.Id);
                    }
                }

                if (distinctDomainIds.Count < 2)
                {
                    throw new InvalidOperationException("When borrowing 3 or more books, they must come from at least 2 distinct categories.");
                }
            }

            // Process loans
            // borrow one by one , other rules woulde be checked in the method borrowbook
            foreach (Guid copyId in copyIdList)
            {
                this.BorrowBook(readerId, copyId);
            }
        }

        private void ValidateStockRule(Guid bookId)
        {
            List<BookCopy>? allCopies = this.copyRepository.Find(c => c.BookEdition != null && c.BookEdition.BookId == bookId).ToList();

            int totalCopies = allCopies.Count;
            if (totalCopies == 0)
            {
                return;
            }

            int readingRoomCopies = allCopies.Count(c => c.IsReadingRoomOnly);

            int availableCirculating = allCopies.Count(c => c.IsAvailable && !c.IsReadingRoomOnly);

            if (readingRoomCopies == totalCopies)
            {
                throw new InvalidOperationException("All copies of this book are for the reading room.");
            }

            double percentage = (double)availableCirculating / totalCopies * 100.0;

            if (percentage < 10.0)
            {
                throw new InvalidOperationException($"Cannot borrow. Stock is too low ({percentage:F1}%). Minimum 10% required.");
            }
        }

        private void ValidateReaderLimits(Reader reader)
        {
            int limitNMC = this.config.MaxBooksPerReader;
            int limitNCZ = this.config.MaxBooksPerDay;

            if (reader.IsLibrarian)
            {
                limitNMC *= 2;
            }

            int activeLoans = this.loanRepository.Find(l => l.ReaderId == reader.Id && l.ReturnDate == null).Count();
            if (activeLoans >= limitNMC)
            {
                throw new InvalidOperationException($"Reader has reached the maximum number of borrowed books ({limitNMC}).");
            }

            if (!reader.IsLibrarian)
            {
                DateTime today = DateTime.Today;
                int loansToday = this.loanRepository.Find(l =>
                    l.ReaderId == reader.Id &&
                    l.LoanDate.Date == today).Count();
                if (loansToday >= limitNCZ)
                {
                    throw new InvalidOperationException($"Reader has reached the daily borrowing limit ({limitNCZ}).");
                }
            }
        }

        private void ValidateDomainHistory(Reader reader, Book book)
        {
            int limitD = this.config.MaxBooksPerDomain;
            int monthsL = this.config.DomainCheckIntervalMonths;

            if (reader.IsLibrarian)
            {
                limitD *= 2;
                monthsL = Math.Max(1, monthsL / 2);
            }

            DateTime sinceDate = DateTime.Now.AddMonths(-monthsL);

            List<Loan>? recentLoans = this.loanRepository.Find(l => l.ReaderId == reader.Id && l.LoanDate >= sinceDate).ToList();

            int countSameDomain = 0;
            foreach (Loan loan in recentLoans)
            {
                Book? loanBook = loan.BookCopy?.BookEdition?.Book;
                if (loanBook == null)
                {
                    continue;
                }

                bool sharesDomain = book.Domains.Any(d1 =>
                    loanBook.Domains.Any(d2 => d1.Id == d2.Id || d1.IsAncestorOf(d2) || d2.IsAncestorOf(d1)));
                if (sharesDomain)
                {
                    countSameDomain++;
                }
            }

            if (countSameDomain >= limitD)
            {
                throw new InvalidOperationException($"Reader has reached the limit ({limitD}) for this domain in the last {monthsL} monts.");
            }
        }

        private void ValidateReborrowDelta(Reader reader, BookCopy copy)
        {
            if (copy.BookEdition == null)
            {
                throw new InvalidOperationException("Book copy is missing its edition.");
            }

            int deltaDays = this.config.ReborrowRestrictedDays;
            if (reader.IsLibrarian)
            {
                deltaDays /= 2;
            }

            Guid targetBookId = copy.BookEdition.BookId;
            DateTime cutoffDate = DateTime.Now.AddDays(-deltaDays);

            Loan? lastLoan = this.loanRepository.Find(l =>
                l.ReaderId == reader.Id &&
                l.BookCopy != null &&
                l.BookCopy.BookEdition != null &&
                l.BookCopy.BookEdition.BookId == targetBookId &&
                l.ReturnDate != null)
                .OrderByDescending(l => l.LoanDate)
                .FirstOrDefault();

            if (lastLoan != null)
            {
                if (lastLoan.LoanDate > cutoffDate)
                {
                    throw new InvalidOperationException($"Cannot re-borrow this book so soon. Must wait {deltaDays} days.");
                }
            }
        }

        private void ValidateLibrarianLimit(Guid librarianId)
        {
            Reader librarian = this.readerRepository.GetById(librarianId)
                ?? throw new ArgumentException("Librarian not found.");

            if (!librarian.IsLibrarian)
            {
                throw new InvalidOperationException("The specified ID does not belong to a Librarian.");
            }

            int limit = this.config.MaxProcessedPerDayLibrarian;
            DateTime today = DateTime.Today;

            int processedToday = this.loanRepository.Find(l =>
                l.LibrarianId == librarianId &&
                l.LoanDate.Date == today).Count();

            if (processedToday >= limit)
            {
                throw new InvalidOperationException($"Librarian has reached the daily processing limit ({limit}).");
            }
        }
    }
}
