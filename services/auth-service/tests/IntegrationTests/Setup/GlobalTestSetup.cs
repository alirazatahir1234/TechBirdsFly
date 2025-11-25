using System;
using Xunit;

namespace AuthService.IntegrationTests.Setup;

/// <summary>
/// Global test collection that sets up environment before any tests run
/// This ensures ASPNETCORE_ENVIRONMENT=Test is set before WebApplicationFactory resolves the Program entry point
/// </summary>
[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class GlobalTestCollection : ICollectionFixture<GlobalTestSetup>
{
    // This class has no code, and all its usage is implicit
    // by the collection definition and GlobalTestSetup fixture
}

/// <summary>
/// Fixture that initializes global test environment
/// Must run BEFORE any WebApplicationFactory is created
/// </summary>
public class GlobalTestSetup : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        // Set environment variable at the VERY START before tests run
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test", EnvironmentVariableTarget.Process);
        Console.WriteLine("✅ Global Test Setup: ASPNETCORE_ENVIRONMENT set to 'Test'");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
