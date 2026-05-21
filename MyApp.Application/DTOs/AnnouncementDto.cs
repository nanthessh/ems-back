namespace MyApp.Application.DTOs
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Priority { get; set; } = "Normal";
        public string? PostedBy { get; set; }
        public DateTime PostedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }
}
