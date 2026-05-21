namespace MyApp.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public decimal TotalSalary { get; set; }
        public int NewEmployeesThisMonth { get; set; }
    }
}
