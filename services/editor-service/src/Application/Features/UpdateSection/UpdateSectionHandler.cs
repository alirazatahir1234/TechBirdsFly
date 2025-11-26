using MediatR;
using TechBirdsFly.EditorService.Domain.Exceptions;
using TechBirdsFly.EditorService.Domain.Interfaces;

namespace TechBirdsFly.EditorService.Application.Features.UpdateSection;

public class UpdateSectionHandler : IRequestHandler<UpdateSectionCommand>
{
    private readonly ISectionRepository _repo;

    public UpdateSectionHandler(ISectionRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _repo.GetByIdAsync(request.Id)
            ?? throw new SectionNotFoundException(request.Id);

        section.UpdateHtml(request.Html);

        if (request.Css != null)
        {
            section.UpdateCss(request.Css);
        }

        await _repo.SaveChangesAsync();
    }
}
