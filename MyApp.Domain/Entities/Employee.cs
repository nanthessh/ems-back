namespace MyApp.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int DepartmentId { get; set; }
        public decimal Salary { get; set; }
        public DateTime JoinedDate { get; set; }
    }
}
