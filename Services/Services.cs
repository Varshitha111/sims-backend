using Microsoft.IdentityModel.Tokens;
using SIMS.DTOs;
using SIMS.Models;
using SIMS.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIMS.Services;

// Auth
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
}

public class AuthService : IAuthService
{
    private readonly IHrRepository _repo;
    private readonly IConfiguration _config;

    public AuthService(IHrRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var hr = await _repo.GetByEmailAsync(dto.Email);
        if (hr == null || !BCrypt.Net.BCrypt.Verify(dto.Password, hr.PasswordHash))
            return null;

        var key = _config["Jwt:Key"] ?? "SuperSecretKey@123SuperSecretKey@123";
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, hr.HrId.ToString()),
                new Claim(ClaimTypes.Name, hr.Name),
                new Claim(ClaimTypes.Email, hr.Email),
                new Claim(ClaimTypes.Role, "HR")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _config["Jwt:Issuer"] ?? "SIMS",
            Audience = _config["Jwt:Audience"] ?? "SIMSUsers"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new LoginResponseDto(tokenHandler.WriteToken(token), hr.Name, hr.Email);
    }
}

// Candidate
public interface ICandidateService
{
    Task<IEnumerable<CandidateResponseDto>> GetAllAsync();
    Task<CandidateResponseDto?> GetByIdAsync(int id);
    Task<CandidateResponseDto> CreateAsync(CandidateCreateDto dto, string? resumePath);
    Task<CandidateResponseDto?> UpdateAsync(int id, CandidateUpdateDto dto, string? resumePath);
    Task<bool> DeleteAsync(int id);
}

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _repo;
    public CandidateService(ICandidateRepository repo) => _repo = repo;

    public async Task<IEnumerable<CandidateResponseDto>> GetAllAsync()
    {
        var candidates = await _repo.GetAllAsync();
        return candidates.Select(ToDto);
    }

    public async Task<CandidateResponseDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        return c == null ? null : ToDto(c);
    }

    public async Task<CandidateResponseDto> CreateAsync(CandidateCreateDto dto, string? resumePath)
    {
        if (!Enum.TryParse<CandidateStatus>(dto.Status, out var status)) status = CandidateStatus.Scheduled;
        var candidate = new Candidate
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Skills = dto.Skills,
            Experience = dto.Experience,
            AppliedRole = dto.AppliedRole,
            Status = status,
            ResumePath = resumePath
        };
        var created = await _repo.CreateAsync(candidate);
        return ToDto(created);
    }

    public async Task<CandidateResponseDto?> UpdateAsync(int id, CandidateUpdateDto dto, string? resumePath)
    {
        var candidate = await _repo.GetByIdAsync(id);
        if (candidate == null) return null;

        if (!Enum.TryParse<CandidateStatus>(dto.Status, out var status)) status = CandidateStatus.Scheduled;
        candidate.FullName = dto.FullName;
        candidate.Email = dto.Email;
        candidate.PhoneNumber = dto.PhoneNumber;
        candidate.Skills = dto.Skills;
        candidate.Experience = dto.Experience;
        candidate.AppliedRole = dto.AppliedRole;
        candidate.Status = status;
        if (resumePath != null) candidate.ResumePath = resumePath;

        var updated = await _repo.UpdateAsync(candidate);
        return ToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static CandidateResponseDto ToDto(Candidate c) => new()
    {
        CandidateId = c.CandidateId,
        FullName = c.FullName,
        Email = c.Email,
        PhoneNumber = c.PhoneNumber,
        Skills = c.Skills,
        Experience = c.Experience,
        AppliedRole = c.AppliedRole,
        ResumePath = c.ResumePath,
        Status = c.Status.ToString(),
        CreatedAt = c.CreatedAt
    };
}

// Interview
public interface IInterviewService
{
    Task<IEnumerable<InterviewResponseDto>> GetAllAsync();
    Task<InterviewResponseDto> CreateAsync(InterviewCreateDto dto);
    Task<InterviewResponseDto?> UpdateAsync(int id, InterviewUpdateDto dto);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _repo;
    private readonly ICandidateRepository _candidateRepo;
    public InterviewService(IInterviewRepository repo, ICandidateRepository candidateRepo)
    {
        _repo = repo;
        _candidateRepo = candidateRepo;
    }

    public async Task<IEnumerable<InterviewResponseDto>> GetAllAsync()
    {
        var interviews = await _repo.GetAllAsync();
        return interviews.Select(ToDto);
    }

    public async Task<InterviewResponseDto> CreateAsync(InterviewCreateDto dto)
    {
        if (!Enum.TryParse<InterviewMode>(dto.InterviewMode, out var mode)) mode = InterviewMode.Online;
        if (!Enum.TryParse<InterviewStatus>(dto.Status, out var status)) status = InterviewStatus.Scheduled;
        if (!TimeSpan.TryParse(dto.InterviewTime, out var time)) time = TimeSpan.Zero;

        var interview = new Interview
        {
            CandidateId = dto.CandidateId,
            Role = dto.Role,
            InterviewDate = dto.InterviewDate,
            InterviewTime = time,
            InterviewMode = mode,
            InterviewerName = dto.InterviewerName,
            MeetingLink = dto.MeetingLink,
            Status = status
        };
        var created = await _repo.CreateAsync(interview);
        await _repo.GetByIdAsync(created.InterviewId); // reload with candidate
        var full = await _repo.GetByIdAsync(created.InterviewId);
        return ToDto(full!);
    }

    public async Task<InterviewResponseDto?> UpdateAsync(int id, InterviewUpdateDto dto)
    {
        var interview = await _repo.GetByIdAsync(id);
        if (interview == null) return null;

        if (!Enum.TryParse<InterviewMode>(dto.InterviewMode, out var mode)) mode = InterviewMode.Online;
        if (!Enum.TryParse<InterviewStatus>(dto.Status, out var status)) status = InterviewStatus.Scheduled;
        if (!TimeSpan.TryParse(dto.InterviewTime, out var time)) time = TimeSpan.Zero;

        interview.CandidateId = dto.CandidateId;
        interview.Role = dto.Role;
        interview.InterviewDate = dto.InterviewDate;
        interview.InterviewTime = time;
        interview.InterviewMode = mode;
        interview.InterviewerName = dto.InterviewerName;
        interview.MeetingLink = dto.MeetingLink;
        interview.Status = status;

        var updated = await _repo.UpdateAsync(interview);
        var full = await _repo.GetByIdAsync(updated.InterviewId);
        return ToDto(full!);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalCandidates = await _candidateRepo.CountAsync();
        var todayCount = await _repo.CountTodayAsync();
        var upcomingCount = await _repo.CountUpcomingAsync();
        var completedCount = await _repo.CountCompletedAsync();
        var todayInterviews = await _repo.GetTodayAsync();

        return new DashboardStatsDto
        {
            TotalCandidates = totalCandidates,
            InterviewsToday = todayCount,
            UpcomingInterviews = upcomingCount,
            CompletedInterviews = completedCount,
            TodayInterviews = todayInterviews.Select(ToDto).ToList()
        };
    }

    private static InterviewResponseDto ToDto(Interview i) => new()
    {
        InterviewId = i.InterviewId,
        CandidateId = i.CandidateId,
        CandidateName = i.Candidate?.FullName ?? "",
        Role = i.Role,
        InterviewDate = i.InterviewDate,
        InterviewTime = i.InterviewTime.ToString(@"hh\:mm"),
        InterviewMode = i.InterviewMode.ToString(),
        InterviewerName = i.InterviewerName,
        MeetingLink = i.MeetingLink,
        Status = i.Status.ToString(),
        CreatedAt = i.CreatedAt
    };
}

// Feedback
public interface IFeedbackService
{
    Task<IEnumerable<FeedbackResponseDto>> GetAllAsync();
    Task<FeedbackResponseDto> CreateAsync(FeedbackCreateDto dto);
}

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _repo;
    private readonly ICandidateRepository _candidateRepo;
    public FeedbackService(IFeedbackRepository repo, ICandidateRepository candidateRepo)
    {
        _repo = repo;
        _candidateRepo = candidateRepo;
    }

    public async Task<IEnumerable<FeedbackResponseDto>> GetAllAsync()
    {
        var feedbacks = await _repo.GetAllAsync();
        return feedbacks.Select(ToDto);
    }

    public async Task<FeedbackResponseDto> CreateAsync(FeedbackCreateDto dto)
    {
        if (!Enum.TryParse<FinalDecision>(dto.FinalDecision, out var decision)) decision = FinalDecision.Hold;
        var feedback = new Feedback
        {
            CandidateId = dto.CandidateId,
            TechnicalRating = dto.TechnicalRating,
            CommunicationRating = dto.CommunicationRating,
            OverallRating = dto.OverallRating,
            Remarks = dto.Remarks,
            FinalDecision = decision
        };
        var created = await _repo.CreateAsync(feedback);

        // Update candidate status based on decision
        var candidate = await _candidateRepo.GetByIdAsync(dto.CandidateId);
        if (candidate != null)
        {
            candidate.Status = decision == FinalDecision.Selected ? CandidateStatus.Selected :
                                decision == FinalDecision.Rejected ? CandidateStatus.Rejected :
                                CandidateStatus.Completed;
            await _candidateRepo.UpdateAsync(candidate);
        }

        var full = (await _repo.GetAllAsync()).First(f => f.FeedbackId == created.FeedbackId);
        return ToDto(full);
    }

    private static FeedbackResponseDto ToDto(Feedback f) => new()
    {
        FeedbackId = f.FeedbackId,
        CandidateId = f.CandidateId,
        CandidateName = f.Candidate?.FullName ?? "",
        AppliedRole = f.Candidate?.AppliedRole ?? "",
        TechnicalRating = f.TechnicalRating,
        CommunicationRating = f.CommunicationRating,
        OverallRating = f.OverallRating,
        Remarks = f.Remarks,
        FinalDecision = f.FinalDecision.ToString(),
        CreatedAt = f.CreatedAt
    };
}
