using AutoMapper;
using GeneratorService.Application.DTOs;
using GeneratorService.Domain.Entities;
using GeneratorService.Domain.ValueObjects;

namespace GeneratorService.Application.Common;

/// <summary>
/// Consolidated AutoMapper profile for all domain entity ↔ DTO mappings
/// Configures automatic mapping of Domain models to Application DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Section → SectionDto
        CreateMap<Section, SectionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.HtmlContent, opt => opt.MapFrom(src => src.Html.Value));

        // Project → GeneratedWebsiteDto
        CreateMap<Project, GeneratedWebsiteDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Industry, opt => opt.MapFrom(src => src.Industry))
            .ForMember(dest => dest.Style, opt => opt.MapFrom(src => src.Style))
            .ForMember(dest => dest.PrimaryColor, opt => opt.MapFrom(src => src.Palette.Primary))
            .ForMember(dest => dest.SecondaryColor, opt => opt.MapFrom(src => src.Palette.Secondary))
            .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections));

        // GeneratedPage → GeneratedWebsiteDto
        CreateMap<GeneratedPage, GeneratedWebsiteDto>()
            .ForMember(dest => dest.FinalHtml, opt => opt.MapFrom(src => src.Html.Value))
            .ForMember(dest => dest.Css, opt => opt.MapFrom(src => src.Css))
            .ForMember(dest => dest.JavaScript, opt => opt.MapFrom(src => src.JavaScript))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                new MetadataDto
                {
                    Title = src.Meta.Title,
                    Description = src.Meta.Description,
                    Keywords = src.Meta.Keywords
                }
            ));

        // Metadata → MetadataDto
        CreateMap<Metadata, MetadataDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Keywords));
    }
}
