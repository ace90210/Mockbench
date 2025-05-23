using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Data.Contexts;
using Mockbench.Shared.Models.Configuration;

namespace Mockbench.Data.Tests;

/// <summary>
/// A test-specific subclass of MockbenchDbContext to verify that specific methods are called.
/// This version is updated to accept the required IOptions<DeploymentConfiguration> dependency.
/// </summary>
public class TestableMockbenchDbContext : MockbenchDbContext
{
    public bool SeedConfigurationAsyncCalled { get; private set; }

    public TestableMockbenchDbContext(DbContextOptions<MockbenchDbContext> options, IOptions<DeploymentConfiguration> deploymentConfigurationOptions)
        : base(options, deploymentConfigurationOptions)
    {
    }

    /// <summary>
    /// Overrides the real method to record that it was called during a test.
    /// </summary>
    public override Task SeedConfigurationAsync()
    {
        SeedConfigurationAsyncCalled = true;
        return Task.CompletedTask;
    }
}
