using Microsoft.EntityFrameworkCore;
using TechBirdsFly.AdminService.Application.Interfaces;
using TechBirdsFly.AdminService.Domain.Entities;

namespace TechBirdsFly.AdminService.Infrastructure.Repositories;

/// <summary>
/// Repository for Role entity persistence and retrieval operations.
/// Implements the IRoleRepository interface for data access abstraction.
/// Includes special handling for system roles which cannot be modified or deleted.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly AdminDbContext _context;

    public RoleRepository(AdminDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return null;

        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .OrderBy(r => r.IsSystem)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (role.IsSystem)
            throw new InvalidOperationException("Cannot create system roles via repository. System roles must be seeded in migrations.");

        _context.Roles.Add(role);
        return role;
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var existingRole = await _context.Roles.FindAsync(new object[] { role.Id }, cancellationToken);
        if (existingRole == null)
            throw new InvalidOperationException($"Role with ID {role.Id} not found");

        if (existingRole.IsSystem)
            throw new InvalidOperationException("Cannot modify system roles. System roles are protected.");

        _context.Roles.Update(role);
        return role;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return;

        var role = await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
        if (role != null)
        {
            if (role.IsSystem)
                throw new InvalidOperationException("Cannot delete system roles. System roles are protected.");

            _context.Roles.Remove(role);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
