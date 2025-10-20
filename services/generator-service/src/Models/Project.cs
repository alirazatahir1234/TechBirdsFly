using System.ComponentModel.DataAnnotations;

namespace GeneratorService.Models
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // pending, processing, completed, failed
        public string? PreviewUrl { get; set; }
        public string? ArtifactUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class GenerateWebsiteJob
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string Status { get; set; } = "queued"; // queued, processing, completed, failed
        public string? GeneratedCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}