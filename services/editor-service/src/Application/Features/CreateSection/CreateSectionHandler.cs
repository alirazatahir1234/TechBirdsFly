using MediatR;
using TechBirdsFly.EditorService.Domain.Entities;
using TechBirdsFly.EditorService.Domain.Interfaces;

namespace TechBirdsFly.EditorService.Application.Features.CreateSection;

public class CreateSectionHandler : IRequestHandler<CreateSectionCommand, Guid>
{
    private readonly ISectionRepository _repo;

    public CreateSectionHandler(ISectionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = new Section(
            request.ProjectId,
            request.Type,
            request.Html,
            request.Order,
            request.Css);

        await _repo.AddAsync(section);
        await _repo.SaveChangesAsync();

        return section.Id;
    }
}
