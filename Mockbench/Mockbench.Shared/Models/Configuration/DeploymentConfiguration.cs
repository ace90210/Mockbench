using System;
using System.Collections.Generic;
using Mockbench.Shared.Models.Utility;

namespace Mockbench.Shared.Models.Configuration;

public class DeploymentConfiguration : ICopyTo<DeploymentConfiguration>
{
    public DatabaseConfig DatabaseConfig { get; set; } = new DatabaseConfig();

#if DEBUG
    public bool? Debug { get; set; } = true;
#else
    public bool? Debug { get; set; } = false;
#endif

    public string PathBase { get; set; }
    
    public string SeedOnStartup { get; set; }

    public string DebuggerUrl { get; set; }

    public ConnectionStringStatus SqlConnectionStatus { get; set; }

    public bool ForceHttps { get; set; } = true;

    public IEnumerable<string> PendingMigrations { get; set; }

    public string DefaultTheme { get; set; }

    public UIMode DefaultUIMode { get; set; } = UIMode.Full;
    
    public DeploymentConfiguration CopyTo(DeploymentConfiguration target)
    {
        if (target == null)
            throw new NotSupportedException($"{nameof(DeploymentConfiguration)}: Cannot copy to a null target");
        
        target.Debug = Debug;
        target.PathBase = PathBase;
        target.DebuggerUrl = DebuggerUrl;
        target.SqlConnectionStatus = SqlConnectionStatus;
        target.ForceHttps = ForceHttps;

        var targetMigrations = new List<string>();

        foreach (var pendingMigration in PendingMigrations)
        {
            targetMigrations.Add(pendingMigration);
        }

        target.PendingMigrations = targetMigrations;

        target.DatabaseConfig = new DatabaseConfig();

        DatabaseConfig.CopyTo(target.DatabaseConfig);
        
        return target;
    }

    public int GetId()
    {
        return 0;
    }
}
