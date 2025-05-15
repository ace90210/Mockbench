#Requires -Version 5.1
<#
.SYNOPSIS
    Automates adding Entity Framework Core migrations for multiple database providers.
.DESCRIPTION
    This script prompts for a migration name and DbContext type (Authentication or Mockbench).
    It then iterates through a list of predefined database providers. For each provider,
    it sets an environment variable, determines the correct DbContext class name (which can
    be provider-specific for Mockbench), sets the correct output directory for migrations,
    runs 'dotnet ef migrations add', and logs any errors.
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
# **TODO: Customize these values to match your project setup!**
$ErrorLogFile = "migration_error.log"
$EnvVarName = "MOCKBENCH_DB_PROVIDER" # The environment variable your application uses

$Providers = @(
    @{ Name = "SQL Server"; EnvValue = "SqlServer";  MockbenchContextPrefix = "SqlServer" }, # Used to form e.g., SqlServerMockbenchContext
    @{ Name = "PostgreSQL"; EnvValue = "PostgreSQL"; MockbenchContextPrefix = "PostgreSQL" },
    @{ Name = "SQLite";     EnvValue = "SQLite";     MockbenchContextPrefix = "SQLite"     }
    # Add or modify providers as needed. Ensure MockbenchContextPrefix is correct.
)

$ContextChoices = @{
    "1" = @{ Name = "Authentication Context"; BaseClassName = "ApplicationDbContext"; OutputDirName = "AuthenticationMigrations" } # TODO: Verify BaseClassName
    "2" = @{ Name = "Mockbench Context";    BaseClassName = "MockbenchDbContext";   OutputDirName = "MockbenchMigrations"    } # TODO: Verify BaseClassName (this is the conceptual base)
    # Add or modify context choices as needed
}
# --- End Configuration ---

# --- Function to log messages ---
function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO", # INFO, WARNING, ERROR
        [System.ConsoleColor]$ForegroundColor = $Host.UI.RawUI.ForegroundColor # Keep current color by default
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
Clear-Content $ErrorLogFile -ErrorAction SilentlyContinue
Write-Host "--- Entity Framework Core Multi-Provider Migration Script ---"

# 1. Get Migration Name
$MigrationName = ""
while ([string]::IsNullOrWhiteSpace($MigrationName)) {
    $MigrationName = Read-Host "Enter the migration name (e.g., AddUserRoles)"
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Warning "Migration name cannot be empty. Please try again."
    }
}

# 2. Get DbContext Choice (Conceptual Choice)
$SelectedContextConfig = $null
$validChoice = $false
do {
    Write-Host "Select the conceptual DbContext for the migration:"
    foreach ($key in $ContextChoices.Keys | Sort-Object) {
        Write-Host "$key. $($ContextChoices[$key].Name) (Base Class: $($ContextChoices[$key].BaseClassName), Output Dir: $($ContextChoices[$key].OutputDirName))"
    }
    $choiceKey = Read-Host "Enter your choice (number)"
    if ($ContextChoices.ContainsKey($choiceKey)) {
        $SelectedContextConfig = $ContextChoices[$choiceKey]
        Write-Host "You selected: $($SelectedContextConfig.Name)"
        $validChoice = $true
    } else {
        Write-Warning "Invalid choice. Please select a valid number from the list."
    }
} while (-not $validChoice)

# 3. Determine Automation Mode
$runAutomated = $Automated.IsPresent
if (-not $runAutomated) {
    $confirmAutomated = Read-Host "Run in automated mode (skips pauses between migrations)? (yes/no) [no]"
    if ($confirmAutomated -eq 'yes') {
        $runAutomated = $true
        Write-Host "Running in automated mode."
    } else {
        Write-Host "Running in interactive mode."
    }
} else {
    Write-Host "Running in automated mode (specified by -Automated switch)."
}

# --- Loop through providers ---
$anyErrorOccurred = $false

try {
    foreach ($provider in $Providers) {
        Write-Host ""
        Write-Log "--- Processing Provider: $($provider.Name) ---"

        # Determine the actual DbContext class name and Output Path for the command
        $actualDbContextForCommand = ""
        $actualMigrationOutputPath = $SelectedContextConfig.OutputDirName # This is now fixed per conceptual context

        if ($SelectedContextConfig.BaseClassName -eq "ApplicationDbContext") {
            $actualDbContextForCommand = "ApplicationDbContext"
        }
        elseif ($SelectedContextConfig.BaseClassName -eq "MockbenchDbContext") {
            # Construct provider-specific DbContext name, e.g., SqlServerMockbenchContext
            $actualDbContextForCommand = "$($provider.MockbenchContextPrefix)$($SelectedContextConfig.BaseClassName)"
        }
        else {
            Write-Log "ERROR: Unknown BaseClassName '$($SelectedContextConfig.BaseClassName)' configured." -Level "ERROR" -ForegroundColor Red
            $anyErrorOccurred = $true
            continue # Skip to next provider
        }

        Write-Log "Using DbContext: '$actualDbContextForCommand'"
        Write-Log "Migration output path will be: '$actualMigrationOutputPath'"

        try {
            # Set environment variable
            $env:$EnvVarName = $provider.EnvValue
            Write-Log "Set environment variable '$EnvVarName' to '$($env:$EnvVarName)'"

            # Construct dotnet ef command arguments
            # TODO: If you need to specify --project or --startup-project, add them here.
            # Example:
            # $arguments = @(
            #     "ef", "migrations", "add", $MigrationName,
            #     "-c", $actualDbContextForCommand,
            #     "-o", $actualMigrationOutputPath,
            #     "--project", "Path/To/YourDataProject.csproj",
            #     "--startup-project", "Path/To/YourStartupApiProject.csproj"
            # )
            $arguments = @(
                "ef", "migrations", "add", $MigrationName,
                "-c", $actualDbContextForCommand,
                "-o", $actualMigrationOutputPath
            )

            Write-Log "Executing: dotnet $($arguments -join ' ')"

            # Execute the command and capture output
            $process = Start-Process dotnet -ArgumentList $arguments -Wait -NoNewWindow -PassThru -RedirectStandardError "ef_error.tmp" -RedirectStandardOutput "ef_output.tmp"
            
            $stdOut = Get-Content "ef_output.tmp" -Raw -ErrorAction SilentlyContinue
            $stdErr = Get-Content "ef_error.tmp" -Raw -ErrorAction SilentlyContinue
            Remove-Item "ef_output.tmp", "ef_error.tmp" -ErrorAction SilentlyContinue


            if ($process.ExitCode -eq 0) {
                Write-Log "Migration for $($provider.Name) (Context: $actualDbContextForCommand) succeeded." -ForegroundColor Green
                if (-not [string]::IsNullOrWhiteSpace($stdOut)) {
                    Write-Host "Output from dotnet ef:"
                    Write-Host $stdOut
                }
                if (-not $runAutomated) {
                    Read-Host "Press Enter to continue to the next migration..."
                }
            } else {
                # Throw an exception to be caught by the outer catch block for this provider
                throw "dotnet ef command failed with exit code $($process.ExitCode)." 
            }
        } catch {
            $anyErrorOccurred = $true
            $errorMessage = "ERROR: Migration for $($provider.Name) (Context: $actualDbContextForCommand) failed.`nEnvironment: $EnvVarName=$($provider.EnvValue)`nCommand: dotnet $($arguments -join ' ')`nException: $($_.Exception.Message)`nStderr: $stdErr`nStdout: $stdOut"
            Write-Log $errorMessage -Level "ERROR" -ForegroundColor Red
            
            if (-not $runAutomated) {
                Read-Host "Error occurred. Press Enter to attempt next migration (if any)..."
            }
        }
    }
}
finally {
    # --- Cleanup ---
    Write-Host ""
    Write-Log "Removing environment variable '$EnvVarName'..."
    Remove-Item "Env:\$EnvVarName" -ErrorAction SilentlyContinue
    Write-Log "Environment variable '$EnvVarName' removed."

    # --- Summary ---
    Write-Host ""
    if ($anyErrorOccurred) {
        Write-Log "One or more migrations failed. Please check '$ErrorLogFile' for details." -Level "WARNING" -ForegroundColor Yellow
    } else {
        Write-Log "All migrations processed successfully for all providers." -ForegroundColor Green
    }
    Write-Host "--- Script Finished ---"
}
