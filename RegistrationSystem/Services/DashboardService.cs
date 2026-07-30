using RegistrationSystem.Data;
using RegistrationSystem.Models;

namespace RegistrationSystem.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public DashboardViewModel GetDashboard()
        {
            var now = DateTime.UtcNow; // get the current UTC date

            // Starts with the first day of the month from eleven months ago.
            // Including the current month, this gives us twelve months in total.

            // since today is July 30, 2026, this produces the first day fo the current month
            // .AddMonths(-11) moves backward eleven months: 2025-08-01
            var firstMonth =
                new DateTime(now.Year, now.Month, 1).AddMonths(-11);

            // find the end of the period, starting form 2025-08-01, adding 12 months gives us 2026-08-01, which is the first day of the month after the range we want to include
            // this date is not included in the report. it is the boundary immediately after hte 12 month period
            // the resulting range is:
            // BorrowDate >= 2025-08-01
            // BorrowDate < 2026-08-01
            var firstMonthAfterRange = firstMonth.AddMonths(12);

            // Gets the number of borrow records created during each month.
            var monthlyCounts = _context.Borrows
                .Where(borrow => // only borrow records inside the selected 12 month range
                    borrow.BorrowDate >= firstMonth &&
                    borrow.BorrowDate < firstMonthAfterRange)
                .GroupBy(borrow => new // groups them by year and month
                {
                    borrow.BorrowDate.Year,
                    borrow.BorrowDate.Month
                })
                .Select(group => new // convert each group into a count
                { // group.Key contains the year and month used to create that group
                    group.Key.Year,
                    group.Key.Month,
                    BorrowCount = group.Count() // counts how many borrow records belong to that month
                })
                .ToList(); // loads the results into memory

            // create the final chart list
            var monthlyStatistics =
                new List<MonthlyDashboardStatisticViewModel>(); // this creates an empty list that will contain exactly 12 items

            // loop through all 12 months
            // adds all twelve months to the result, including months
            // that have no borrowing records.
            for (var monthOffset = 0; // from 0
                 monthOffset < 12; // to 11
                 monthOffset++)
            {// calculate the month for the current iteration
                var month = firstMonth.AddMonths(monthOffset);

                // look for DB results matching that month
                var matchingMonth = monthlyCounts.FirstOrDefault(item =>
                    item.Year == month.Year &&
                    item.Month == month.Month);

                // add the month to the final list
                // this creates a one MonthDashboardStatisticViewModel and adds it to the final list
                monthlyStatistics.Add(
                    new MonthlyDashboardStatisticViewModel
                    {
                        Month = month.ToString("yyyy-MM"), // converts the date into a chart friendly label
                        BorrowCount =
                            matchingMonth?.BorrowCount ?? 0 // handling months with no data
                    });                                     // get BorrowCount only if matchingMonth is not null, if the value is null, use 0 instead
            }

            // Counts the historical borrow records for each active book
            // and returns the five most borrowed books.
            var mostBorrowedBooks =
                (from borrow in _context.Borrows
                 join book in _context.Books
                     on borrow.BookId equals book.Id
                 where !book.IsDeleted
                 group borrow by new
                 {
                     book.Id,
                     book.Title
                 }
                 into borrowGroup
                 orderby borrowGroup.Count() descending
                 select new DashboardItemViewModel
                 {
                     Name = borrowGroup.Key.Title,
                     Count = borrowGroup.Count()
                 })
                .Take(5)
                .ToList();

            // Counts borrowing activity for each active category
            // and returns the five most popular categories.
            var topCategories =
                (from borrow in _context.Borrows
                 join book in _context.Books
                     on borrow.BookId equals book.Id
                 join category in _context.Categories
                     on book.CategoryId equals (int?)category.Id
                 where
                     !book.IsDeleted &&
                     !category.IsDeleted
                 group borrow by new
                 {
                     category.Id,
                     category.Name
                 }
                 into borrowGroup
                 orderby borrowGroup.Count() descending
                 select new DashboardItemViewModel
                 {
                     Name = borrowGroup.Key.Name,
                     Count = borrowGroup.Count()
                 })
                .Take(5)
                .ToList();

            // Finds the five active members with the highest number
            // of books that have not yet been returned.
            var activeUsers =
                (from borrow in _context.Borrows
                 join person in _context.Persons
                     on borrow.PersonId equals person.Id
                 where
                     borrow.ReturnDate == null &&
                     !person.IsDeleted
                 group borrow by new
                 {
                     person.Id,
                     person.FullName
                 }
                 into borrowGroup
                 orderby borrowGroup.Count() descending
                 select new DashboardItemViewModel
                 {
                     Name = borrowGroup.Key.FullName,
                     Count = borrowGroup.Count()
                 })
                .Take(5)
                .ToList();

            return new DashboardViewModel
            {
                // Archived books and people are not included in these totals.
                TotalBooks = _context.Books.Count(book =>
                    !book.IsDeleted),

                TotalMembers = _context.Persons.Count(person =>
                    !person.IsDeleted),

                ActiveBorrowings = _context.Borrows.Count(borrow =>
                    borrow.ReturnDate == null),

                OverdueBooks = _context.Borrows.Count(borrow =>
                    borrow.ReturnDate == null &&
                    borrow.DueDate < now),

                MostBorrowedBooks = mostBorrowedBooks, // this groups all historical borrow records by BookId and returns the five highest counts
                TopCategories = topCategories, // this connects Borrows, Books and categories, it then groups the borrowing records by Category and selects the five most popular categories
                ActiveUsers = activeUsers, // this chart means members who currently have books that have not been returned
                MonthlyStatistics = monthlyStatistics
            };
        }
    }
}