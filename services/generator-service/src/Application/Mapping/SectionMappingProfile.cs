using AutoMapper;
using GeneratorService.Domain.Entities;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Mapping;

/// <summary>
/// AutoMapper profile for Section entity mappings
/// </summary>
public class SectionMappingProfile : Profile
{
    public SectionMappingProfile()
    {
        // Section → SectionDto
        CreateMap<Section, SectionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.HtmlContent, opt => opt.MapFrom(src => src.Html.Value))
            .ForMember(dest => dest.CssClass, opt => opt.MapFrom(src => src.CssClass ?? ""));

        // SectionDto → Section (if needed for input)
        CreateMap<SectionDto, Section>()
            .ConstructUsing((dto, ctx) =>
                new Section(
                    dto.ProjectId,
                    (Domain.ValueObjects.SectionType)Enum.Parse(typeof(Domain.ValueObjects.SectionType), dto.Type),
                    Domain.ValueObjects.HtmlContent.Create(dto.HtmlContent),
                    dto.CssClass
                )
            );
    }
}
