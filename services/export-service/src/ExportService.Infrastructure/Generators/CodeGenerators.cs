using ExportService.Application.Interfaces;
using ExportService.Application.Models;

namespace ExportService.Infrastructure.Generators;

/// <summary>
/// Base implementation for code generators
/// </summary>
public abstract class BaseCodeGenerator : ICodeGenerator
{
    protected abstract Task<string> GenerateCodeAsync(ProjectDto project);
    protected virtual string GetOutputFileName() => "index.html";

    public async Task<byte[]> GenerateAsync(ProjectDto project, string framework, CancellationToken cancellationToken = default)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        var code = await GenerateCodeAsync(project);
        return CreateZipArchive(code, GetOutputFileName());
    }

    /// <summary>
    /// Creates a zip archive containing the generated code
    /// </summary>
    protected byte[] CreateZipArchive(string code, string fileName)
    {
        using var memoryStream = new MemoryStream();
        using (var zipArchive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zipArchive.CreateEntry(fileName);
            using (var streamWriter = new StreamWriter(entry.Open()))
            {
                streamWriter.Write(code);
            }
        }

        return memoryStream.ToArray();
    }
}

/// <summary>
/// Generates plain HTML code
/// </summary>
public class HtmlCodeGenerator : BaseCodeGenerator
{
    protected override Task<string> GenerateCodeAsync(ProjectDto project)
    {
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{project.Name}</title>
    <style>
        {project.Css}
    </style>
</head>
<body>
    {project.Html}
</body>
</html>";

        return Task.FromResult(html);
    }
}

/// <summary>
/// Generates React JSX code
/// </summary>
public class ReactCodeGenerator : BaseCodeGenerator
{
    protected override string GetOutputFileName() => "App.jsx";

    protected override Task<string> GenerateCodeAsync(ProjectDto project)
    {
        var jsx = $@"import React from 'react';
import './App.css';

export default function App() {{
  return (
    <div className=""app"">
      {project.Html}
    </div>
  );
}}
";

        return Task.FromResult(jsx);
    }
}

/// <summary>
/// Generates Next.js page code
/// </summary>
public class NextJsCodeGenerator : BaseCodeGenerator
{
    protected override string GetOutputFileName() => "page.jsx";

    protected override Task<string> GenerateCodeAsync(ProjectDto project)
    {
        var nextJs = $@"'use client';

import React from 'react';
import './page.css';

export default function Page() {{
  return (
    <main className=""main"">
      {project.Html}
    </main>
  );
}}
";

        return Task.FromResult(nextJs);
    }
}

/// <summary>
/// Factory for creating appropriate code generator
/// </summary>
public class CodeGeneratorFactory
{
    public static ICodeGenerator CreateGenerator(string framework) =>
        framework.ToLowerInvariant() switch
        {
            "html" => new HtmlCodeGenerator(),
            "react" => new ReactCodeGenerator(),
            "nextjs" => new NextJsCodeGenerator(),
            _ => throw new ArgumentException($"Unsupported framework: {framework}")
        };
}

/// <summary>
/// Implementation of ICodeGenerator that delegates to specific generators based on framework
/// </summary>
public class FrameworkAwareCodeGenerator : ICodeGenerator
{
    public Task<byte[]> GenerateAsync(ProjectDto project, string framework, CancellationToken cancellationToken = default)
    {
        var generator = CodeGeneratorFactory.CreateGenerator(framework);
        return generator.GenerateAsync(project, framework, cancellationToken);
    }
}
