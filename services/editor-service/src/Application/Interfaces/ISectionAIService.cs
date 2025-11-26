namespace TechBirdsFly.EditorService.Application.Interfaces;

public interface ISectionAIService
{
    Task<string> RegenerateHtmlAsync(string type, string oldHtml);
}
