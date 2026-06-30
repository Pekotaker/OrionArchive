using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrionArchive.Web.Data;
using OrionArchive.Web.Models;
using OrionArchive.Web.ViewModels;

namespace OrionArchive.Web.Controllers;

public class CoursesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CoursesController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .Include(c => c.CourseCategories)
            .ThenInclude(cc => cc.Category)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _context.Courses
            .Include(c => c.CourseCategories)
            .ThenInclude(cc => cc.Category)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        course.Lessons = course.Lessons
            .OrderBy(l => l.OrderIndex)
            .ToList();

        return View(course);
    }

    public IActionResult Create()
    {
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var course = new Course
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            BannerPath = await SaveBannerAsync(viewModel.BannerFile)
        };

        await AttachCategoriesAsync(course, viewModel.CategoryNames);

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = course.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses
            .Include(c => c.CourseCategories)
            .ThenInclude(cc => cc.Category)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        var viewModel = new CourseFormViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            ExistingBannerPath = course.BannerPath,
            CategoryNames = string.Join(", ", course.CourseCategories.Select(cc => cc.Category.Name))
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var course = await _context.Courses
            .Include(c => c.CourseCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        course.Title = viewModel.Title;
        course.Description = viewModel.Description;

        var newBannerPath = await SaveBannerAsync(viewModel.BannerFile);
        if (newBannerPath != null)
        {
            course.BannerPath = newBannerPath;
        }

        course.CourseCategories.Clear();
        await AttachCategoriesAsync(course, viewModel.CategoryNames);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = course.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveBannerAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }

    private async Task AttachCategoriesAsync(Course course, string? categoryNames)
    {
        if (string.IsNullOrWhiteSpace(categoryNames))
        {
            return;
        }

        var names = categoryNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var name in names)
        {
            var loweredName = name.ToLower();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == loweredName);

            if (category == null)
            {
                category = new Category
                {
                    Name = name
                };
            }

            course.CourseCategories.Add(new CourseCategory
            {
                Course = course,
                Category = category
            });
        }
    }
}