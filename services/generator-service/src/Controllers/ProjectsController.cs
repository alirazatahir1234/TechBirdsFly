using GeneratorService.Models;
using Microsoft.AspNetCore.Mvc;
using GeneratorService.Data;
using GeneratorService.Services;
using Microsoft.EntityFrameworkCore;

namespace GeneratorService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly GeneratorDbContext _db;
        private readonly IGeneratorService _generator;
        private readonly IMessagePublisher _publisher;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            GeneratorDbContext db,
            IGeneratorService generator,
            IMessagePublisher publisher,
            ILogger<ProjectsController> logger)
        {
            _db = db;
            _generator = generator;
            _publisher = publisher;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectRequest req)
        {
            try
            {
                // Extract userId from JWT claim (or request header for now)
                var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
                if (!Guid.TryParse(userIdHeader, out var userId))
                {
                    return Unauthorized(new { error = "Invalid or missing X-User-Id header" });
                }

                var project = new Project
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = req.Name,
                    Prompt = req.Prompt,
                    Status = "pending"
                };

                _db.Projects.Add(project);
                await _db.SaveChangesAsync();

                // Create a job for async processing
                var job = new GenerateWebsiteJob
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    UserId = userId,
                    Prompt = req.Prompt,
                    Status = "queued"
                };

                _db.GenerateWebsiteJobs.Add(job);
                await _db.SaveChangesAsync();

                // Publish to message bus for worker to pick up
                try
                {
                    await _publisher.PublishJobAsync("generate_website_jobs", new
                    {
                        job.Id,
                        job.ProjectId,
                        job.Prompt,
                        job.UserId
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ publish failed, job queued locally");
                }

                return Ok(new
                {
                    projectId = project.Id,
                    jobId = job.Id,
                    status = project.Status,
                    message = "Project created and queued for generation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            try
            {
                var project = await _db.Projects.FindAsync(id);
                if (project == null)
                    return NotFound(new { error = "Project not found" });

                var job = await _db.GenerateWebsiteJobs
                    .FirstOrDefaultAsync(j => j.ProjectId == id);

                return Ok(new
                {
                    project.Id,
                    project.Name,
                    project.Status,
                    project.PreviewUrl,
                    project.ArtifactUrl,
                    jobStatus = job?.Status ?? "unknown",
                    project.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching project");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadProject(Guid id)
        {
            try
            {
                var project = await _db.Projects.FindAsync(id);
                if (project == null)
                    return NotFound(new { error = "Project not found" });

                if (string.IsNullOrEmpty(project.ArtifactUrl))
                    return BadRequest(new { error = "Project not ready for download" });

                // In production, this would return a signed URL from Blob Storage
                // For now, return mock data
                return Ok(new { downloadUrl = project.ArtifactUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading project");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public record CreateProjectRequest(string Name, string Prompt);
}
