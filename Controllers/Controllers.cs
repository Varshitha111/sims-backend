using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.DTOs;
using SIMS.Services;

namespace SIMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    public AuthController(IAuthService service) => _service = service;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest(new { message = "Email and password are required." });

        var result = await _service.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _service;
    private readonly IWebHostEnvironment _env;
    public CandidatesController(ICandidateService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CandidateCreateDto dto, IFormFile? resume)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        string? resumePath = null;
        if (resume != null)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "resumes");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}_{resume.FileName}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await resume.CopyToAsync(stream);
            resumePath = $"/resumes/{fileName}";
        }

        var result = await _service.CreateAsync(dto, resumePath);
        return CreatedAtAction(nameof(GetById), new { id = result.CandidateId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] CandidateUpdateDto dto, IFormFile? resume)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        string? resumePath = null;
        if (resume != null)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "resumes");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}_{resume.FileName}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await resume.CopyToAsync(stream);
            resumePath = $"/resumes/{fileName}";
        }

        var result = await _service.UpdateAsync(id, dto, resumePath);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _service;
    public InterviewsController(IInterviewService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InterviewCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] InterviewUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _service;
    public FeedbackController(IFeedbackService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FeedbackCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.TechnicalRating < 1 || dto.TechnicalRating > 10 ||
            dto.CommunicationRating < 1 || dto.CommunicationRating > 10 ||
            dto.OverallRating < 1 || dto.OverallRating > 10)
            return BadRequest(new { message = "Ratings must be between 1 and 10." });

        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IInterviewService _service;
    public DashboardController(IInterviewService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetStats() => Ok(await _service.GetDashboardStatsAsync());
}
