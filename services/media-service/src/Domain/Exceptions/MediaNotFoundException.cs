namespace TechBirdsFly.MediaService.Domain.Exceptions;

public class MediaNotFoundException : Exception
{
    public MediaNotFoundException(Guid id)
        : base($"Media file '{id}' not found.") { }

    public MediaNotFoundException(string message)
        : base(message) { }
}
