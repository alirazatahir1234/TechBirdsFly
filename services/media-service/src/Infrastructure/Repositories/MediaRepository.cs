using Microsoft.EntityFrameworkCore;
using TechBirdsFly.MediaService.Domain.Entities;
using TechBirdsFly.MediaService.Domain.Interfaces;
using TechBirdsFly.MediaService.Infrastructure.Persistence;

namespace TechBirdsFly.MediaService.Infrastructure.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly MediaDbContext _context;

    public MediaRepository(MediaDbContext context)
    {
        _context = context;
    }

    public async Task<MediaFile?> GetByIdAsync(Guid id)
    {
        return await _context.MediaFiles.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<MediaFile>> GetAllAsync()
    {
        return await _context.MediaFiles.OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public async Task AddAsync(MediaFile file)
    {
        await _context.MediaFiles.AddAsync(file);
    }

    public async Task DeleteAsync(MediaFile file)
    {
        _context.MediaFiles.Remove(file);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
