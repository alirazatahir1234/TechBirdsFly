namespace GeneratorService.Domain.Exceptions;

/// <summary>
/// Base exception for all domain layer exceptions
/// Represents violations of domain business rules
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when project validation fails
/// </summary>
public class InvalidProjectException : DomainException
{
    public InvalidProjectException(string message) : base($"Invalid project: {message}") { }
}

/// <summary>
/// Exception thrown when section validation fails
/// </summary>
public class InvalidSectionException : DomainException
{
    public InvalidSectionException(string message) : base($"Invalid section: {message}") { }
}

/// <summary>
/// Exception thrown when generation fails
/// </summary>
public class GenerationFailedException : DomainException
{
    public GenerationFailedException(string message) : base($"Generation failed: {message}") { }
}

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class ResourceNotFoundException : DomainException
{
    public ResourceNotFoundException(string resourceName, Guid id)
        : base($"{resourceName} with ID {id} not found") { }
}
