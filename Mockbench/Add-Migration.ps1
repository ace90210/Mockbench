#Requires -Version 5.1
<#
.SYNOPSIS
    Automates adding Entity Framework Core migrations for multiple database providers.
.DESCRIPTION
    This script prompts for a migration name and DbContext type (Authentication or Mockbench).
    It then iterates through a list of predefined database providers. For each provider,
    it sets an environment variable, determines the correct DbContext class name and --project path
    (which can be provider-specific for Mockbench), sets the correct output directory for migrations,
    runs 'dotnet ef migrations add' with verbose output, and logs any errors.
    It supports an automated mode to skip interactive pauses.
.PARAMETER Automated
    If specified, the script runs in non-interactive mode, skipping pauses
    between provider migrations.
.EXAMPLE
    .\New-EfMigration.ps1
    Runs the script in interactive mode.
.EXAMPLE
    .\New-EfMigration.ps1 -Automated
    Runs the script in automated mode.
#>
param (
    [switch]$Automated
)

# --- Configuration ---
$ErrorLogFile = "migration_error.log"
$EnvVarName = "DeploymentConfiguration__DatabaseConfig__Provider" 

$CommonDataProjectFilePath = ".\Mockbench.Data\Mockbench.Data.csproj"
$StartupProjectFilePath = ".\Mockbench\Mockbench.csproj" 

$Providers = @(  
	@{ Name = "Postgres"; EnvValue = "Postgres"; MockbenchContextPrefix = "Postgres"; ProviderDataProjectFolder = "Mockbench.Data.Postgres" },
    @{ Name = "SQL Server"; EnvValue = "SqlServer";  MockbenchContextPrefix = "SqlServer"; ProviderDataProjectFolder = "Mockbench.Data.SqlServer" },
    @{ Name = "SQLite";     EnvValue = "SQLite";     MockbenchContextPrefix = "SQLite";     ProviderDataProjectFolder = "Mockbench.Data.Sqlite" } 
)

$ContextChoices = @{
    "1" = @{ Name = "Authentication Context"; BaseClassName = "ApplicationDbContext"; OutputDirName = "AuthenticationMigrations"; IsProviderSpecific = $false }
    "2" = @{ Name = "Mockbench Context";    BaseClassName = "MockbenchDbContext";   OutputDirName = "MockbenchMigrations";    IsProviderSpecific = $true  }
}
# --- End Configuration ---

# --- Function to log messages ---
function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO", 
        [System.ConsoleColor]$ForegroundColor = $Host.UI.RawUI.ForegroundColor
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    
    if ($PSBoundParameters.ContainsKey('ForegroundColor')) {
        Write-Host $logEntry -ForegroundColor $ForegroundColor
    } else {
        Write-Host $logEntry
    }
    
    if ($Level -eq "ERROR" -or $Level -eq "WARNING") {
        Add-Content -Path $ErrorLogFile -Value $logEntry
    }
}

# --- Main Script ---
if ([string]::IsNullOrWhiteSpace($CommonDataProjectFilePath)) {
    Write-Error "ERROR: \$CommonDataProjectFilePath is not set. Configure common data project .csproj path."
    exit 1
}
if ([string]::IsNullOrWhiteSpace($StartupProjectFilePath)) {
    Write-Error "ERROR: \$StartupProjectFilePath is not set. Configure startup project .csproj path."
    exit 1
}

Clear-Content $ErrorLogFile -ErrorAction SilentlyContinue
Write-Host "--- Entity Framework Core Multi-Provider Migration Script ---"

$MigrationName = ""
while ([string]::IsNullOrWhiteSpace($MigrationName)) {
    $MigrationName = Read-Host "Enter the migration name (e.g., AddUserRoles)"
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Warning "Migration name cannot be empty."
    }
}

$SelectedContextConfig = $null
$validChoice = $false
do {
    Write-Host "Select the DbContext for the migration:"
    foreach ($key in $ContextChoices.Keys | Sort-Object) {
        Write-Host "$key. $($ContextChoices[$key].Name) (Base Class: $($ContextChoices[$key].BaseClassName), Output Dir: $($ContextChoices[$key].OutputDirName))"
    }
    $choiceKey = Read-Host "Enter your choice (number)"
    if ($ContextChoices.ContainsKey($choiceKey)) {
        $SelectedContextConfig = $ContextChoices[$choiceKey]
        Write-Host "You selected: $($SelectedContextConfig.Name)"
        $validChoice = $true
    } else {
        Write-Warning "Invalid choice."
    }
} while (-not $validChoice)

$runAutomated = $Automated.IsPresent
if (-not $runAutomated) {
    $confirmAutomated = Read-Host "Run in automated mode (skips pauses)? (yes/no) [no]"
    if ($confirmAutomated -eq 'yes') {
        $runAutomated = $true
        Write-Host "Running in automated mode."
    } else {
        Write-Host "Running in interactive mode."
    }
} else {
    Write-Host "Running in automated mode (specified by -Automated switch)."
}

$anyErrorOccurred = $false

try {
    foreach ($provider in $Providers) {
        Write-Host ""
        Write-Log "--- Processing Provider: $($provider.Name) ---"

        $actualDbContextForCommand = ""
        $actualMigrationOutputPath = $SelectedContextConfig.OutputDirName
        $projectPathForCommand = $CommonDataProjectFilePath 

        if ($SelectedContextConfig.IsProviderSpecific) {
            $actualDbContextForCommand = "$($provider.MockbenchContextPrefix)$($SelectedContextConfig.BaseClassName)"
            if ([string]::IsNullOrWhiteSpace($provider.ProviderDataProjectFolder)) {
                Write-Log "ERROR: ProviderDataProjectFolder not configured for provider '$($provider.Name)'." -Level "ERROR" -ForegroundColor Red
                $anyErrorOccurred = $true
                continue
            }
            $projectPathForCommand = ".\$($provider.ProviderDataProjectFolder)\$($provider.ProviderDataProjectFolder).csproj"
            Write-Log "Using provider-specific data project: '$projectPathForCommand'"
        }
        elseif ($SelectedContextConfig.BaseClassName -eq "ApplicationDbContext") { 
            $actualDbContextForCommand = "ApplicationDbContext"
            Write-Log "Using common data project: '$projectPathForCommand'"
        }
        else {
            Write-Log "ERROR: Unknown BaseClassName '$($SelectedContextConfig.BaseClassName)' or context config error." -Level "ERROR" -ForegroundColor Red
            $anyErrorOccurred = $true
            continue 
        }

        Write-Log "Using DbContext: '$actualDbContextForCommand'"
        Write-Log "Migration output path will be: '$actualMigrationOutputPath'"

        try {
            Set-Item -Path "Env:\$($EnvVarName)" -Value $provider.EnvValue
            $currentEnvValue = (Get-Item -Path "Env:\$($EnvVarName)").Value
            Write-Log "Set environment variable '$EnvVarName' to '$currentEnvValue'"

            $arguments = @(
                "ef", "migrations", "add", $MigrationName,
                "--context", $actualDbContextForCommand,
                "--output-dir", $actualMigrationOutputPath,
                "--project", $projectPathForCommand, 
                "--startup-project", $StartupProjectFilePath,
                "--verbose" # Added verbose flag for more detailed EF Core output
            )
            
            Write-Log "Executing: dotnet $($arguments -join ' ')"
            Write-Log "About to call Start-Process..." # New log message

            $process = Start-Process dotnet -ArgumentList $arguments -Wait -NoNewWindow -PassThru -RedirectStandardError "ef_error.tmp" -RedirectStandardOutput "ef_output.tmp"
            
            # New log message - this will tell us if Start-Process -Wait returned
            Write-Log "Start-Process call returned. Process ID: $($process.Id). Checking Exit Code..." 
            
            $stdOut = Get-Content "ef_output.tmp" -Raw -ErrorAction SilentlyContinue
            $stdErr = Get-Content "ef_error.tmp" -Raw -ErrorAction SilentlyContinue
            Remove-Item "ef_output.tmp", "ef_error.tmp" -ErrorAction SilentlyContinue

            Write-Log "Process Exit Code: $($process.ExitCode)" # Log the actual exit code received

            if ($process.ExitCode -eq 0) {
                Write-Log "Migration for $($provider.Name) (Context: $actualDbContextForCommand) succeeded." -ForegroundColor Green
                if (-not [string]::IsNullOrWhiteSpace($stdOut)) {
                    Write-Host "Output from dotnet ef (verbose):"
                    Write-Host $stdOut
                }
                if (-not $runAutomated) {
                    Read-Host "Press Enter to continue to the next migration..."
                }
            } else {
                throw "dotnet ef command failed with exit code $($process.ExitCode)." 
            }
        } catch {
            $anyErrorOccurred = $true
            $envValueForErrorLog = ""
            try {
                $envValueForErrorLog = (Get-Item -Path "Env:\$($EnvVarName)" -ErrorAction SilentlyContinue).Value
            } catch {} 

            $errorMessage = "ERROR: Migration for $($provider.Name) (Context: $actualDbContextForCommand) failed.`nEnvironment: $EnvVarName=$envValueForErrorLog`nCommand: dotnet $($arguments -join ' ')`nException: $($_.Exception.Message)`nStderr: $stdErr`nStdout: $stdOut"
            Write-Log $errorMessage -Level "ERROR" -ForegroundColor Red
            
            if (-not $runAutomated) {
                Read-Host "Error occurred. Press Enter to attempt next migration (if any)..."
            }
        }
    }
}
finally {
    Write-Host ""
    Write-Log "Removing environment variable '$EnvVarName'..."
    Remove-Item "Env:\$($EnvVarName)" -ErrorAction SilentlyContinue 
    Write-Log "Environment variable '$EnvVarName' removed."

    Write-Host ""
    if ($anyErrorOccurred) {
        Write-Log "One or more migrations failed. Check '$ErrorLogFile' for details." -Level "WARNING" -ForegroundColor Yellow
    } else {
        Write-Log "All migrations processed successfully for all providers." -ForegroundColor Green
    }
    Write-Host "--- Script Finished ---"
}
