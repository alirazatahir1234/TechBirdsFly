namespace GeneratorService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using GeneratorService.Domain.Entities;
using GeneratorService.Application.Interfaces;
using GeneratorService.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Template aggregate
/// </summary>
public class TemplateRepository : ITemplateRepository
{
    private readonly GeneratorDbContext _context;

    public TemplateRepository(GeneratorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Template>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Template>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Template>> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        var templateType = Enum.Parse<TemplateType>(type);
        return await _context.Templates
            .AsNoTracking()
            .Where(t => t.Type == templateType && t.IsActive)
            .OrderByDescending(t => t.UseCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Template>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .AsNoTracking()
            .Where(t => t.Category == category && t.IsActive)
            .OrderByDescending(t => t.UseCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default)
    {
        await _context.Templates.AddAsync(template, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        _context.Templates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.Templates.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (template != null)
        {
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// Repository implementation for Project aggregate
/// </summary>
public class ProjectRepository : IProjectRepository
{
    private readonly GeneratorDbContext _context;

    public ProjectRepository(GeneratorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var projectStatus = Enum.Parse<ProjectStatus>(status);
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.Status == projectStatus)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// Repository implementation for Generation aggregate
/// </summary>
public class GenerationRepository : IGenerationRepository
{
    private readonly GeneratorDbContext _context;

    public GenerationRepository(GeneratorDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Generation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Generations
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Generation>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Generations
            .AsNoTracking()
            .Where(g => g.ProjectId == projectId)
            .OrderByDescending(g => g.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Generation>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var generationStatus = Enum.Parse<GenerationStatus>(status);
        return await _context.Generations
            .AsNoTracking()
            .Where(g => g.Status == generationStatus)
            .OrderByDescending(g => g.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Generation> CreateAsync(Generation generation, CancellationToken cancellationToken = default)
    {
        await _context.Generations.AddAsync(generation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return generation;
    }

    public async Task<Generation> UpdateAsync(Generation generation, CancellationToken cancellationToken = default)
    {
        _context.Generations.Update(generation);
        await _context.SaveChangesAsync(cancellationToken);
        return generation;
    }
}
