using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrionArchive.Web.Data;
using OrionArchive.Web.Models;
using OrionArchive.Web.Services;
using OrionArchive.Web.ViewModels;

namespace OrionArchive.Web.Controllers;

public class QuizzesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public QuizzesController(AppDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Details(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        quiz.Answers = quiz.Answers
            .OrderBy(a => a.Id)
            .ToList();

        return View(quiz);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
        {
            return NotFound();
        }

        var viewModel = new QuizFormViewModel
        {
            CourseId = course.Id,
            CourseTitle = course.Title
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizFormViewModel viewModel)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == viewModel.CourseId);

        if (course == null)
        {
            return NotFound();
        }

        viewModel.CourseTitle = course.Title;

        ValidateAnswers(viewModel.Answers);

        var bannerPath = await _fileUploadService.SaveImageAsync(
            viewModel.BannerFile,
            ModelState,
            nameof(viewModel.BannerFile));

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var maxOrder = await _context.Lessons
            .Where(l => l.CourseId == viewModel.CourseId)
            .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

        var quiz = new Quiz
        {
            CourseId = viewModel.CourseId,
            Title = viewModel.Title,
            Content = viewModel.Content,
            BannerPath = bannerPath,
            OrderIndex = maxOrder + 1,
            Answers = viewModel.Answers
                .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                .Select(a => new QuizAnswer
                {
                    Content = a.Content.Trim(),
                    IsCorrect = a.IsCorrect
                })
                .ToList()
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Courses", new { id = viewModel.CourseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var answers = quiz.Answers
            .OrderBy(a => a.Id)
            .Select(a => new QuizAnswerInput
            {
                Id = a.Id,
                Content = a.Content,
                IsCorrect = a.IsCorrect
            })
            .ToList();

        while (answers.Count < 4)
        {
            answers.Add(new QuizAnswerInput());
        }

        var viewModel = new QuizFormViewModel
        {
            Id = quiz.Id,
            CourseId = quiz.CourseId,
            CourseTitle = quiz.Course.Title,
            Title = quiz.Title,
            Content = quiz.Content,
            ExistingBannerPath = quiz.BannerPath,
            Answers = answers
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuizFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        viewModel.CourseTitle = quiz.Course.Title;
        viewModel.ExistingBannerPath = quiz.BannerPath;

        ValidateAnswers(viewModel.Answers);

        var newBannerPath = await _fileUploadService.SaveImageAsync(
            viewModel.BannerFile,
            ModelState,
            nameof(viewModel.BannerFile));

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        quiz.Title = viewModel.Title;
        quiz.Content = viewModel.Content;

        if (newBannerPath != null)
        {
            quiz.BannerPath = newBannerPath;
        }

        _context.QuizAnswers.RemoveRange(quiz.Answers);

        quiz.Answers = viewModel.Answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Content))
            .Select(a => new QuizAnswer
            {
                QuizId = quiz.Id,
                Content = a.Content.Trim(),
                IsCorrect = a.IsCorrect
            })
            .ToList();

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = quiz.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var courseId = quiz.CourseId;

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int quizId, List<int>? selectedAnswerIds)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            return NotFound();
        }

        selectedAnswerIds ??= [];

        var correctAnswerIds = quiz.Answers
            .Where(a => a.IsCorrect)
            .Select(a => a.Id)
            .OrderBy(id => id)
            .ToList();

        var selectedIds = selectedAnswerIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var isCorrect = correctAnswerIds.SequenceEqual(selectedIds);

        var selectedAnswers = quiz.Answers
            .Where(a => selectedIds.Contains(a.Id))
            .Select(a => a.Content)
            .ToList();

        var correctAnswers = quiz.Answers
            .Where(a => a.IsCorrect)
            .Select(a => a.Content)
            .ToList();

        var result = new QuizResultViewModel
        {
            QuizId = quiz.Id,
            CourseId = quiz.CourseId,
            QuizTitle = quiz.Title,
            IsCorrect = isCorrect,
            SelectedAnswers = selectedAnswers,
            CorrectAnswers = correctAnswers
        };

        return View("Result", result);
    }

    private void ValidateAnswers(List<QuizAnswerInput> answers)
    {
        var filledAnswers = answers
            .Where(a => !string.IsNullOrWhiteSpace(a.Content))
            .ToList();

        if (filledAnswers.Count < 2)
        {
            ModelState.AddModelError("", "A quiz must have at least two answers.");
            return;
        }

        if (!filledAnswers.Any(a => a.IsCorrect))
        {
            ModelState.AddModelError("", "A quiz must have at least one correct answer.");
        }

        if (answers.Any(a => string.IsNullOrWhiteSpace(a.Content) && a.IsCorrect))
        {
            ModelState.AddModelError("", "A blank answer cannot be marked as correct.");
        }
    }
}