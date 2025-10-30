using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;

namespace AuthService.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation coordinating multiple repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _context;
    private IUserRepository? _userRepository;

    public UnitOfWork(AuthDbContext context)
    {
        _context = context;
    }

    public IUserRepository UserRepository =>
        _userRepository ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
