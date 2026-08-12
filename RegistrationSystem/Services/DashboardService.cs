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

        public DashboardViewModel GetDashboard(
            int trendMonths = 12)
        {
            var now = DateTime.UtcNow; // get the current UTC date

            // Keeps the selected period between one and twelve months,
            // even if someone manually changes the value in the URL.
            var selectedTrendMonths =
                Math.Clamp(trendMonths, 1, 12); // if the month is less than 1, it will be set to 1, if it is greater than 12, it will be set to 12

            // Starts with the first day of the earliest selected month.
            // Subtracting one accounts for the current month being included.
            var firstMonth =
                new DateTime(now.Year, now.Month, 1)
                    .AddMonths(-(selectedTrendMonths - 1));

            // Calculates the exclusive boundary immediately after
            // the selected reporting period.
            var firstMonthAfterRange =
                firstMonth.AddMonths(selectedTrendMonths);

            // Gets the number of borrow records created during each month.
            var monthlyCounts = _context.Borrows
                .Where(borrow => // only borrow records inside the selected month range
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
                new List<MonthlyDashboardStatisticViewModel>(); // this creates an empty list for the selected number of months

            // Loops through every month in the selected period.
            // Months without borrowing records are also included.
            for (var monthOffset = 0; // starts with the earliest month
                 monthOffset < selectedTrendMonths; // continues through the current month
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
                SelectedTrendMonths = selectedTrendMonths, // this is the number of months selected for the trend chart, it is used to display the correct number of months in the chart
                MonthlyStatistics = monthlyStatistics
            };
        }
    }
}