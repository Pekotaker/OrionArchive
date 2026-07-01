namespace OrionArchive.Web.ViewModels;

public class QuizResultViewModel
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public List<string> SelectedAnswers { get; set; } = [];

    public List<string> CorrectAnswers { get; set; } = [];
}