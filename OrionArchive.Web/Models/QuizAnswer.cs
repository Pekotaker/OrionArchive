using System.ComponentModel.DataAnnotations;

namespace OrionArchive.Web.Models;

public class QuizAnswer : AuditableEntity
{
    public int Id { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}