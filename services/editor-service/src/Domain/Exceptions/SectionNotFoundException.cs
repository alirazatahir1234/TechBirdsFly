namespace TechBirdsFly.EditorService.Domain.Exceptions;

public class SectionNotFoundException : Exception
{
    public SectionNotFoundException(Guid id)
        : base($"Section '{id}' not found.") { }

    public SectionNotFoundException(string message)
        : base(message) { }
}
