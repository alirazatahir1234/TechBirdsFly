using MediatR;
using TechBirdsFly.EditorService.Domain.Entities;
using TechBirdsFly.EditorService.Domain.Interfaces;

namespace TechBirdsFly.EditorService.Application.Features.ListSections;

public class ListSectionsHandler : IRequestHandler<ListSectionsQuery, List<Section>>
{
    private readonly ISectionRepository _repo;

    public ListSectionsHandler(ISectionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Section>> Handle(ListSectionsQuery request, CancellationToken cancellationToken)
    {
        return await _repo.GetByProjectIdAsync(request.ProjectId);
    }
}
