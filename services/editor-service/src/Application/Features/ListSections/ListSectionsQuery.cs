using MediatR;
using TechBirdsFly.EditorService.Domain.Entities;

namespace TechBirdsFly.EditorService.Application.Features.ListSections;

public record ListSectionsQuery(Guid ProjectId) : IRequest<List<Section>>;
