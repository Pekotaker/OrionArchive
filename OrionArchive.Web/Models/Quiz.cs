namespace OrionArchive.Web.Models;

public class Quiz : Lesson
{
    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
}