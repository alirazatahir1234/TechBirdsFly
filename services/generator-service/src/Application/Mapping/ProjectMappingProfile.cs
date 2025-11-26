using AutoMapper;
using GeneratorService.Domain.Entities;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Mapping;

/// <summary>
/// AutoMapper profile for Project entity mappings
/// </summary>
public class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        // Project → ProjectDto
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.PrimaryColor, opt => opt.MapFrom(src => src.Palette.Primary))
            .ForMember(dest => dest.SecondaryColor, opt => opt.MapFrom(src => src.Palette.Secondary))
            .ForMember(dest => dest.AccentColor, opt => opt.MapFrom(src => src.Palette.Accent))
            .ForMember(dest => dest.SectionCount, opt => opt.MapFrom(src => src.Sections.Count));

        // ProjectDto → Project (if needed for input)
        CreateMap<ProjectDto, Project>()
            .ConstructUsing((dto, ctx) => 
                new Project(
                    dto.Name,
                    dto.Industry,
                    dto.Style,
                    Domain.ValueObjects.ColorPalette.Create(
                        dto.PrimaryColor,
                        dto.SecondaryColor,
                        dto.AccentColor
                    ),
                    dto.Description
                )
            );
    }
}
