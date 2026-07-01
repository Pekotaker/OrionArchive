using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.ViewModels;

public class QuizFormViewModel
{
    public int? Id { get; set; }

    public int CourseId { get; set; }

    public string? CourseTitle { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public IFormFile? BannerFile { get; set; }

    public string? ExistingBannerPath { get; set; }

    public List<QuizAnswerInput> Answers { get; set; } =
    [
        new QuizAnswerInput(),
        new QuizAnswerInput(),
        new QuizAnswerInput(),
        new QuizAnswerInput()
    ];
}

public class QuizAnswerInput
{
    public int? Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}