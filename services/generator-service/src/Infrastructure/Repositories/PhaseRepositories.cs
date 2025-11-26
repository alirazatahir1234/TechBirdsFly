using GeneratorService.Domain.Entities;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Domain.ValueObjects;

namespace GeneratorService.Infrastructure.Repositories;

/// <summary>
/// In-memory implementations of repositories for Phase 2
/// Will be replaced with EF Core implementations in Phase 4
/// </summary>

public class ProjectRepository : IProjectRepository
{
    private readonly Dictionary<Guid, Project> _projects = new();

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_projects.Values.AsEnumerable());
    }

    public Task<IEnumerable<Project>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_projects.Values.Where(p => p.Industry == industry).AsEnumerable());
    }

    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        _projects.Remove(projectId);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_projects.ContainsKey(projectId));
    }
}

public class SectionRepository : ISectionRepository
{
    private readonly Dictionary<Guid, Section> _sections = new();

    public Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _sections.TryGetValue(id, out var section);
        return Task.FromResult(section);
    }

    public Task<IEnumerable<Section>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sections.Values.Where(s => s.ProjectId == projectId).AsEnumerable());
    }

    public Task<IEnumerable<Section>> GetByTypeAsync(Guid projectId, SectionType type, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sections.Values.Where(s => s.ProjectId == projectId && s.Type == type).AsEnumerable());
    }

    public Task AddAsync(Section section, CancellationToken cancellationToken = default)
    {
        _sections[section.Id] = section;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Section section, CancellationToken cancellationToken = default)
    {
        _sections[section.Id] = section;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        _sections.Remove(sectionId);
        return Task.CompletedTask;
    }

    public Task DeleteByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var toDelete = _sections.Values.Where(s => s.ProjectId == projectId).Select(s => s.Id).ToList();
        foreach (var id in toDelete)
        {
            _sections.Remove(id);
        }
        return Task.CompletedTask;
    }
}

public class GeneratedPageRepository : IGeneratedPageRepository
{
    private readonly Dictionary<Guid, GeneratedPage> _pages = new();

    public Task<GeneratedPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _pages.TryGetValue(id, out var page);
        return Task.FromResult(page);
    }

    public Task<IEnumerable<GeneratedPage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_pages.Values.AsEnumerable());
    }

    public Task<IEnumerable<GeneratedPage>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_pages.Values.Where(p => p.IsPublished).AsEnumerable());
    }

    public Task AddAsync(GeneratedPage page, CancellationToken cancellationToken = default)
    {
        _pages[page.Id] = page;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(GeneratedPage page, CancellationToken cancellationToken = default)
    {
        _pages[page.Id] = page;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        _pages.Remove(pageId);
        return Task.CompletedTask;
    }
}
