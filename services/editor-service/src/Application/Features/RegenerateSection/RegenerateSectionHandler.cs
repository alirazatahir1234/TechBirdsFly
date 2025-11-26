using MediatR;
using TechBirdsFly.EditorService.Application.Interfaces;
using TechBirdsFly.EditorService.Domain.Exceptions;
using TechBirdsFly.EditorService.Domain.Interfaces;

namespace TechBirdsFly.EditorService.Application.Features.RegenerateSection;

public class RegenerateSectionHandler : IRequestHandler<RegenerateSectionCommand, string>
{
    private readonly ISectionRepository _repo;
    private readonly ISectionAIService _ai;

    public RegenerateSectionHandler(ISectionRepository repo, ISectionAIService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    public async Task<string> Handle(RegenerateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _repo.GetByIdAsync(request.SectionId)
            ?? throw new SectionNotFoundException(request.SectionId);

        var newHtml = await _ai.RegenerateHtmlAsync(section.Type, section.Html);

        section.UpdateHtml(newHtml);
        await _repo.SaveChangesAsync();

        return newHtml;
    }
}
