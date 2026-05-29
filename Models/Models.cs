namespace SIMS.Models;

public class Hr
{
    public int HrId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Candidate
{
    public int CandidateId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public int Experience { get; set; }
    public string AppliedRole { get; set; } = string.Empty;
    public string? ResumePath { get; set; }
    public CandidateStatus Status { get; set; } = CandidateStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}

public class Interview
{
    public int InterviewId { get; set; }
    public int CandidateId { get; set; }
    public Candidate? Candidate { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime InterviewDate { get; set; }
    public TimeSpan InterviewTime { get; set; }
    public InterviewMode InterviewMode { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Feedback
{
    public int FeedbackId { get; set; }
    public int CandidateId { get; set; }
    public Candidate? Candidate { get; set; }
    public int TechnicalRating { get; set; }
    public int CommunicationRating { get; set; }
    public int OverallRating { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public FinalDecision FinalDecision { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum CandidateStatus { Scheduled, Completed, Selected, Rejected }
public enum InterviewMode { Online, Offline }
public enum InterviewStatus { Scheduled, InProgress, Completed, Cancelled }
public enum FinalDecision { Selected, Rejected, Hold }
