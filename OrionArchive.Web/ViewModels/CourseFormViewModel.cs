using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.ViewModels;

public class CourseFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public IFormFile? BannerFile { get; set; }

    public string? ExistingBannerPath { get; set; }

    [Display(Name = "Categories")]
    public string? CategoryNames { get; set; }
}