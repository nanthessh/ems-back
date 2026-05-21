namespace MyApp.Application.DTOs
{
    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int MaxDays { get; set; }
    }

    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public int LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public string? AdminNote { get; set; }
        public DateTime AppliedOn { get; set; }
        public DateTime? ReviewedOn { get; set; }
    }

    public class ApplyLeaveDto
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = null!;
    }

    public class UpdateLeaveStatusDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public string? AdminNote { get; set; }
    }

    public class LeaveBalanceDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = null!;
        public int MaxDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
    }
}
