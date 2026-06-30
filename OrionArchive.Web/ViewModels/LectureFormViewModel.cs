using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.ViewModels;

public class LectureFormViewModel
{
    public int? Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public IFormFile? BannerFile { get; set; }

    public string? ExistingBannerPath { get; set; }
}