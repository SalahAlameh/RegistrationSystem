namespace RegistrationSystem.Models
{
    public class DashboardViewModel //main delivery box for the dashboard page
    {
        public int TotalBooks { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBooks { get; set; }

        // it also contains the collections needed by the reports and charts
        public List<DashboardItemViewModel> MostBorrowedBooks { get; set; } = new();
        public List<DashboardItemViewModel> TopCategories { get; set; } = new();
        public List<DashboardItemViewModel> ActiveUsers { get; set; } = new();
        public List<MonthlyDashboardStatisticViewModel> MonthlyStatistics { get; set; } = new();
    }

    // this reusable class represents a name and a count
    public class DashboardItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // this class represents a month and the number of borrows in that month
    public class MonthlyDashboardStatisticViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
    }
}