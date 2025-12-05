using Microsoft.EntityFrameworkCore;
using TemplateService.Domain.Entities;
using TemplateService.Domain.Interfaces;
using TemplateService.Infrastructure.Data;

namespace TemplateService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for template persistence
/// </summary>
public class TemplateRepository : ITemplateRepository
{
    private readonly TemplateDbContext _dbContext;

    public TemplateRepository(TemplateDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateTemplateAsync(Template template)
    {
        await _dbContext.Templates.AddAsync(template);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdatePreviewUrlAsync(Guid templateId, string url)
    {
        var template = await _dbContext.Templates.FindAsync(templateId);
        if (template == null)
            throw new KeyNotFoundException($"Template with ID {templateId} not found");

        template.PreviewImageUrl = url;
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddFileAsync(TemplateFile file)
    {
        await _dbContext.TemplateFiles.AddAsync(file);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Template>> GetTemplatesAsync(string? category = null, string? search = null)
    {
        var query = _dbContext.Templates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category.ToLower());

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));

        return await query.Include(t => t.Files).OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<Template?> GetTemplateByIdAsync(Guid id)
    {
        return await _dbContext.Templates
            .Include(t => t.Files)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
