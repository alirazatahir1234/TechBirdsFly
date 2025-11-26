using AutoMapper;
using GeneratorService.Domain.Entities;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Mapping;

/// <summary>
/// AutoMapper profile for GeneratedPage entity mappings
/// </summary>
public class GeneratedPageMappingProfile : Profile
{
    public GeneratedPageMappingProfile()
    {
        // GeneratedPage → GeneratedPageDto
        CreateMap<GeneratedPage, GeneratedPageDto>()
            .ForMember(dest => dest.Html, opt => opt.MapFrom(src => src.Html.Value))
            .ForMember(dest => dest.MetaTitle, opt => opt.MapFrom(src => src.Meta.Title))
            .ForMember(dest => dest.MetaDescription, opt => opt.MapFrom(src => src.Meta.Description))
            .ForMember(dest => dest.MetaKeywords, opt => opt.MapFrom(src => src.Meta.Keywords));

        // GeneratedPageDto → GeneratedPage (if needed for input)
        CreateMap<GeneratedPageDto, GeneratedPage>()
            .ConstructUsing((dto, ctx) =>
                new GeneratedPage(
                    dto.Title,
                    Domain.ValueObjects.HtmlContent.Create(dto.Html),
                    dto.Css,
                    dto.JavaScript,
                    Domain.ValueObjects.Metadata.Create(
                        dto.MetaTitle,
                        dto.MetaDescription,
                        dto.MetaKeywords
                    )
                )
            );
    }
}
