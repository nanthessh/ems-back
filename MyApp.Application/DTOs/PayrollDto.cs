namespace MyApp.Application.DTOs
{
    public class PayrollDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public string? Email { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HouseAllowance { get; set; }
        public decimal TransportAllow { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? PaidOn { get; set; }
        public DateTime GeneratedOn { get; set; }
    }

    public class GeneratePayrollDto
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal HouseAllowance { get; set; }
        public decimal TransportAllow { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
    }

    public class UpdatePayrollStatusDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
    }
}
