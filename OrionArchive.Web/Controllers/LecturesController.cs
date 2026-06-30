using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrionArchive.Web.Data;
using OrionArchive.Web.Models;
using OrionArchive.Web.ViewModels;
using OrionArchive.Web.Services;

namespace OrionArchive.Web.Controllers;

public class LecturesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public LecturesController(AppDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Details(int id)
    {
        var lecture = await _context.Lectures
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lecture == null)
        {
            return NotFound();
        }

        return View(lecture);
    }

    public async Task<IActionResult> Create(int courseId)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);

        if (!courseExists)
        {
            return NotFound();
        }

        var viewModel = new LectureFormViewModel
        {
            CourseId = courseId
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LectureFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var courseExists = await _context.Courses.AnyAsync(c => c.Id == viewModel.CourseId);

        if (!courseExists)
        {
            return NotFound();
        }

        var maxOrder = await _context.Lessons
            .Where(l => l.CourseId == viewModel.CourseId)
            .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

        var bannerPath = await _fileUploadService.SaveImageAsync(
            viewModel.BannerFile,
            ModelState,
            nameof(viewModel.BannerFile));

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var lecture = new Lecture
        {
            CourseId = viewModel.CourseId,
            Title = viewModel.Title,    
            Content = viewModel.Content,
            BannerPath = bannerPath,
            OrderIndex = maxOrder + 1
        };

        _context.Lectures.Add(lecture);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Courses", new { id = viewModel.CourseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lecture = await _context.Lectures
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lecture == null)
        {
            return NotFound();
        }

        var viewModel = new LectureFormViewModel
        {
            Id = lecture.Id,
            CourseId = lecture.CourseId,
            Title = lecture.Title,
            Content = lecture.Content,
            ExistingBannerPath = lecture.BannerPath
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LectureFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var lecture = await _context.Lectures
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lecture == null)
        {
            return NotFound();
        }

        lecture.Title = viewModel.Title;
        lecture.Content = viewModel.Content;

        var newBannerPath = await _fileUploadService.SaveImageAsync(
            viewModel.BannerFile,
            ModelState,
            nameof(viewModel.BannerFile));

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        if (newBannerPath != null)
        {
            lecture.BannerPath = newBannerPath;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = lecture.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var lecture = await _context.Lectures
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lecture == null)
        {
            return NotFound();
        }

        return View(lecture);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lecture = await _context.Lectures
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lecture == null)
        {
            return NotFound();
        }

        var courseId = lecture.CourseId;

        _context.Lectures.Remove(lecture);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
}