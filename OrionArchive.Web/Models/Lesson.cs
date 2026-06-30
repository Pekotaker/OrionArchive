using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.Models;

public abstract class Lesson : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? BannerPath { get; set; }

    public int OrderIndex { get; set; }
}