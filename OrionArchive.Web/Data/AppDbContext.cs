using Microsoft.EntityFrameworkCore;
using OrionArchive.Web.Models;

namespace OrionArchive.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();

    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Lecture> Lectures => Set<Lecture>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseCategory>()
            .HasKey(cc => new { cc.CourseId, cc.CategoryId });

        modelBuilder.Entity<CourseCategory>()
            .HasOne(cc => cc.Course)
            .WithMany(c => c.CourseCategories)
            .HasForeignKey(cc => cc.CourseId);

        modelBuilder.Entity<CourseCategory>()
            .HasOne(cc => cc.Category)
            .WithMany(c => c.CourseCategories)
            .HasForeignKey(cc => cc.CategoryId);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Lesson>()
            .ToTable("Lessons")
            .HasDiscriminator<string>("LessonType")
            .HasValue<Lecture>("Lecture")
            .HasValue<Quiz>("Quiz");

        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Quiz)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}