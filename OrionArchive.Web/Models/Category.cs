using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.Models;

public class Category : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
}