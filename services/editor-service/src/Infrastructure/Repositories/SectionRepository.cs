using Microsoft.EntityFrameworkCore;
using TechBirdsFly.EditorService.Domain.Entities;
using TechBirdsFly.EditorService.Domain.Interfaces;
using TechBirdsFly.EditorService.Infrastructure.Persistence;

namespace TechBirdsFly.EditorService.Infrastructure.Repositories;

public class SectionRepository : ISectionRepository
{
    private readonly EditorDbContext _db;

    public SectionRepository(EditorDbContext db)
    {
        _db = db;
    }

    public async Task<Section?> GetByIdAsync(Guid id)
        => await _db.Sections.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<List<Section>> GetByProjectIdAsync(Guid projectId)
        => await _db.Sections
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Order)
            .ToListAsync();

    public async Task AddAsync(Section section)
        => await _db.Sections.AddAsync(section);

    public async Task DeleteAsync(Section section)
        => _db.Sections.Remove(section);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
