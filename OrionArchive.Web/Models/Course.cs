using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.Models;

public class Course : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? BannerPath { get; set; }

    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}