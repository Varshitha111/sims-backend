using Microsoft.EntityFrameworkCore;
using SIMS.Models;

namespace SIMS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Hr> HrUsers { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>(e =>
        {
            e.HasKey(c => c.CandidateId);
            e.Property(c => c.FullName).IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).IsRequired().HasMaxLength(200);
            e.Property(c => c.Skills).HasMaxLength(1000);
            e.Property(c => c.AppliedRole).HasMaxLength(200);
            e.Property(c => c.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Interview>(e =>
        {
            e.HasKey(i => i.InterviewId);
            e.HasOne(i => i.Candidate).WithMany(c => c.Interviews).HasForeignKey(i => i.CandidateId).OnDelete(DeleteBehavior.Cascade);
            e.Property(i => i.InterviewMode).HasConversion<string>();
            e.Property(i => i.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Feedback>(e =>
        {
            e.HasKey(f => f.FeedbackId);
            e.HasOne(f => f.Candidate).WithMany(c => c.Feedbacks).HasForeignKey(f => f.CandidateId).OnDelete(DeleteBehavior.Cascade);
            e.Property(f => f.FinalDecision).HasConversion<string>();
        });
    }
}

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!context.HrUsers.Any())
        {
            context.HrUsers.Add(new Hr
            {
                Name = "Admin HR",
                Email = "admin@sims.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        if (!context.Candidates.Any())
        {
            var candidates = new List<Candidate>
            {
                new() { FullName = "Arjun Sharma", Email = "arjun@email.com", PhoneNumber = "9876543210", Skills = "React, Node.js, MongoDB", Experience = 3, AppliedRole = "Full Stack Developer", Status = CandidateStatus.Scheduled },
                new() { FullName = "Priya Patel", Email = "priya@email.com", PhoneNumber = "9876543211", Skills = "Java, Spring Boot, MySQL", Experience = 5, AppliedRole = "Backend Developer", Status = CandidateStatus.Selected },
                new() { FullName = "Rahul Verma", Email = "rahul@email.com", PhoneNumber = "9876543212", Skills = "Python, Django, PostgreSQL", Experience = 2, AppliedRole = "Python Developer", Status = CandidateStatus.Completed },
                new() { FullName = "Sneha Reddy", Email = "sneha@email.com", PhoneNumber = "9876543213", Skills = "Angular, TypeScript, Firebase", Experience = 4, AppliedRole = "Frontend Developer", Status = CandidateStatus.Rejected },
                new() { FullName = "Kiran Kumar", Email = "kiran@email.com", PhoneNumber = "9876543214", Skills = "C#, .NET, Azure, SQL Server", Experience = 6, AppliedRole = "Senior .NET Developer", Status = CandidateStatus.Scheduled },
            };
            context.Candidates.AddRange(candidates);
            await context.SaveChangesAsync();

            var today = DateTime.Today;
            var interviews = new List<Interview>
            {
                new() { CandidateId = candidates[0].CandidateId, Role = "Full Stack Developer", InterviewDate = today, InterviewTime = new TimeSpan(10, 0, 0), InterviewMode = InterviewMode.Online, InterviewerName = "Mr. Suresh", MeetingLink = "https://meet.google.com/abc-def", Status = InterviewStatus.Scheduled },
                new() { CandidateId = candidates[1].CandidateId, Role = "Backend Developer", InterviewDate = today.AddDays(-2), InterviewTime = new TimeSpan(14, 0, 0), InterviewMode = InterviewMode.Offline, InterviewerName = "Ms. Lakshmi", Status = InterviewStatus.Completed },
                new() { CandidateId = candidates[2].CandidateId, Role = "Python Developer", InterviewDate = today.AddDays(1), InterviewTime = new TimeSpan(11, 30, 0), InterviewMode = InterviewMode.Online, InterviewerName = "Mr. Ravi", MeetingLink = "https://zoom.us/j/123", Status = InterviewStatus.Scheduled },
                new() { CandidateId = candidates[4].CandidateId, Role = "Senior .NET Developer", InterviewDate = today, InterviewTime = new TimeSpan(15, 0, 0), InterviewMode = InterviewMode.Online, InterviewerName = "Mr. Vijay", MeetingLink = "https://teams.microsoft.com/l/meetup", Status = InterviewStatus.Scheduled },
            };
            context.Interviews.AddRange(interviews);
            await context.SaveChangesAsync();

            context.Feedbacks.Add(new Feedback
            {
                CandidateId = candidates[1].CandidateId,
                TechnicalRating = 9,
                CommunicationRating = 8,
                OverallRating = 9,
                Remarks = "Excellent candidate with strong backend skills.",
                FinalDecision = FinalDecision.Selected
            });
            await context.SaveChangesAsync();
        }
    }
}
