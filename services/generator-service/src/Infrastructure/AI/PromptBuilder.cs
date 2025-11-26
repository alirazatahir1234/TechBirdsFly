namespace GeneratorService.Infrastructure.AI;

/// <summary>
/// Builds prompts for AI text generation
/// Implements prompt engineering best practices
/// </summary>
public interface IPromptBuilder
{
    IPromptBuilder SetContext(string context);
    IPromptBuilder SetTask(string task);
    IPromptBuilder AddConstraint(string constraint);
    IPromptBuilder SetFormat(string format);
    string Build();
    void Clear();
}

public class PromptBuilder : IPromptBuilder
{
    private readonly List<string> _parts = new();
    private string? _context;
    private string? _task;
    private readonly List<string> _constraints = new();
    private string? _format;

    public IPromptBuilder SetContext(string context)
    {
        _context = context;
        return this;
    }

    public IPromptBuilder SetTask(string task)
    {
        _task = task;
        return this;
    }

    public IPromptBuilder AddConstraint(string constraint)
    {
        _constraints.Add(constraint);
        return this;
    }

    public IPromptBuilder SetFormat(string format)
    {
        _format = format;
        return this;
    }

    public string Build()
    {
        var prompt = new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(_context))
        {
            prompt.AppendLine($"Context: {_context}");
            prompt.AppendLine();
        }

        if (!string.IsNullOrEmpty(_task))
        {
            prompt.AppendLine($"Task: {_task}");
            prompt.AppendLine();
        }

        if (_constraints.Any())
        {
            prompt.AppendLine("Constraints:");
            foreach (var constraint in _constraints)
            {
                prompt.AppendLine($"- {constraint}");
            }
            prompt.AppendLine();
        }

        if (!string.IsNullOrEmpty(_format))
        {
            prompt.AppendLine($"Format: {_format}");
            prompt.AppendLine();
        }

        return prompt.ToString().Trim();
    }

    public void Clear()
    {
        _context = null;
        _task = null;
        _constraints.Clear();
        _format = null;
        _parts.Clear();
    }
}
