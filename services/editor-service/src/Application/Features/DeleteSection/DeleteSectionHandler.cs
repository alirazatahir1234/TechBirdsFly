using MediatR;
using TechBirdsFly.EditorService.Domain.Exceptions;
using TechBirdsFly.EditorService.Domain.Interfaces;

namespace TechBirdsFly.EditorService.Application.Features.DeleteSection;

public class DeleteSectionHandler : IRequestHandler<DeleteSectionCommand>
{
    private readonly ISectionRepository _repo;

    public DeleteSectionHandler(ISectionRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _repo.GetByIdAsync(request.Id)
            ?? throw new SectionNotFoundException(request.Id);

        await _repo.DeleteAsync(section);
        await _repo.SaveChangesAsync();
    }
}
