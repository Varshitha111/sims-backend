namespace SIMS.DTOs;

// Auth
public record LoginRequestDto(string Email, string Password);
public record LoginResponseDto(string Token, string Name, string Email);

// Candidate
public class CandidateCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public int Experience { get; set; }
    public string AppliedRole { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
}

public class CandidateUpdateDto : CandidateCreateDto { }

public class CandidateResponseDto
{
    public int CandidateId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public int Experience { get; set; }
    public string AppliedRole { get; set; } = string.Empty;
    public string? ResumePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Interview
public class InterviewCreateDto
{
    public int CandidateId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime InterviewDate { get; set; }
    public string InterviewTime { get; set; } = string.Empty;
    public string InterviewMode { get; set; } = string.Empty;
    public string InterviewerName { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public string Status { get; set; } = "Scheduled";
}

public class InterviewUpdateDto : InterviewCreateDto { }

public class InterviewResponseDto
{
    public int InterviewId { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime InterviewDate { get; set; }
    public string InterviewTime { get; set; } = string.Empty;
    public string InterviewMode { get; set; } = string.Empty;
    public string InterviewerName { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Feedback
public class FeedbackCreateDto
{
    public int CandidateId { get; set; }
    public int TechnicalRating { get; set; }
    public int CommunicationRating { get; set; }
    public int OverallRating { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string FinalDecision { get; set; } = string.Empty;
}

public class FeedbackResponseDto
{
    public int FeedbackId { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string AppliedRole { get; set; } = string.Empty;
    public int TechnicalRating { get; set; }
    public int CommunicationRating { get; set; }
    public int OverallRating { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string FinalDecision { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Dashboard
public class DashboardStatsDto
{
    public int TotalCandidates { get; set; }
    public int InterviewsToday { get; set; }
    public int UpcomingInterviews { get; set; }
    public int CompletedInterviews { get; set; }
    public List<InterviewResponseDto> TodayInterviews { get; set; } = new();
}
