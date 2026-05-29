using Microsoft.EntityFrameworkCore;
using SIMS.Data;
using SIMS.Models;

namespace SIMS.Repositories;

// Interfaces
public interface IHrRepository
{
    Task<Hr?> GetByEmailAsync(string email);
}

public interface ICandidateRepository
{
    Task<IEnumerable<Candidate>> GetAllAsync();
    Task<Candidate?> GetByIdAsync(int id);
    Task<Candidate> CreateAsync(Candidate candidate);
    Task<Candidate> UpdateAsync(Candidate candidate);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
}

public interface IInterviewRepository
{
    Task<IEnumerable<Interview>> GetAllAsync();
    Task<Interview?> GetByIdAsync(int id);
    Task<Interview> CreateAsync(Interview interview);
    Task<Interview> UpdateAsync(Interview interview);
    Task<IEnumerable<Interview>> GetTodayAsync();
    Task<IEnumerable<Interview>> GetUpcomingAsync();
    Task<int> CountCompletedAsync();
    Task<int> CountTodayAsync();
    Task<int> CountUpcomingAsync();
}

public interface IFeedbackRepository
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback> CreateAsync(Feedback feedback);
}

// Implementations
public class HrRepository : IHrRepository
{
    private readonly AppDbContext _ctx;
    public HrRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Hr?> GetByEmailAsync(string email) =>
        await _ctx.HrUsers.FirstOrDefaultAsync(h => h.Email == email);
}

public class CandidateRepository : ICandidateRepository
{
    private readonly AppDbContext _ctx;
    public CandidateRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Candidate>> GetAllAsync() =>
        await _ctx.Candidates.OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<Candidate?> GetByIdAsync(int id) =>
        await _ctx.Candidates.FindAsync(id);

    public async Task<Candidate> CreateAsync(Candidate candidate)
    {
        _ctx.Candidates.Add(candidate);
        await _ctx.SaveChangesAsync();
        return candidate;
    }

    public async Task<Candidate> UpdateAsync(Candidate candidate)
    {
        _ctx.Candidates.Update(candidate);
        await _ctx.SaveChangesAsync();
        return candidate;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var c = await _ctx.Candidates.FindAsync(id);
        if (c == null) return false;
        _ctx.Candidates.Remove(c);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync() => await _ctx.Candidates.CountAsync();
}

public class InterviewRepository : IInterviewRepository
{
    private readonly AppDbContext _ctx;
    public InterviewRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Interview>> GetAllAsync() =>
        await _ctx.Interviews.Include(i => i.Candidate).OrderByDescending(i => i.InterviewDate).ToListAsync();

    public async Task<Interview?> GetByIdAsync(int id) =>
        await _ctx.Interviews.Include(i => i.Candidate).FirstOrDefaultAsync(i => i.InterviewId == id);

    public async Task<Interview> CreateAsync(Interview interview)
    {
        _ctx.Interviews.Add(interview);
        await _ctx.SaveChangesAsync();
        return interview;
    }

    public async Task<Interview> UpdateAsync(Interview interview)
    {
        _ctx.Interviews.Update(interview);
        await _ctx.SaveChangesAsync();
        return interview;
    }

    public async Task<IEnumerable<Interview>> GetTodayAsync()
    {
        var today = DateTime.Today;
        return await _ctx.Interviews
            .Include(i => i.Candidate)
            .Where(i => i.InterviewDate.Date == today)
            .OrderBy(i => i.InterviewTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Interview>> GetUpcomingAsync()
    {
        var today = DateTime.Today;
        return await _ctx.Interviews
            .Include(i => i.Candidate)
            .Where(i => i.InterviewDate.Date >= today && i.Status != InterviewStatus.Completed && i.Status != InterviewStatus.Cancelled)
            .OrderBy(i => i.InterviewDate).ThenBy(i => i.InterviewTime)
            .ToListAsync();
    }

    public async Task<int> CountCompletedAsync() =>
        await _ctx.Interviews.CountAsync(i => i.Status == InterviewStatus.Completed);

    public async Task<int> CountTodayAsync() =>
        await _ctx.Interviews.CountAsync(i => i.InterviewDate.Date == DateTime.Today);

    public async Task<int> CountUpcomingAsync() =>
        await _ctx.Interviews.CountAsync(i => i.InterviewDate.Date > DateTime.Today && i.Status == InterviewStatus.Scheduled);
}

public class FeedbackRepository : IFeedbackRepository
{
    private readonly AppDbContext _ctx;
    public FeedbackRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Feedback>> GetAllAsync() =>
        await _ctx.Feedbacks.Include(f => f.Candidate).OrderByDescending(f => f.CreatedAt).ToListAsync();

    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        _ctx.Feedbacks.Add(feedback);
        await _ctx.SaveChangesAsync();
        return feedback;
    }
}
