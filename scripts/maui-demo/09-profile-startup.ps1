[CmdletBinding()]
param(
    [ValidateSet('Auto', 'Android', 'iOS')]
    [string]$Platform = 'Auto',
    [string]$Device,
    [string]$Duration = '00:00:15'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')
$project = Join-Path $PSScriptRoot '..\..\SimpleApp\SimpleApp.csproj'
$outputDirectory = Join-Path $PSScriptRoot '..\..\artifacts\profiles'

function Assert-ProfilingTools {
    $missingTools = @('dotnet-trace', 'dotnet-dsrouter') | Where-Object {
        $null -eq (Get-Command $_ -ErrorAction SilentlyContinue)
    }

    if ($missingTools.Count -gt 0) {
        throw "Install the required profiling tools with: dotnet tool install --global dotnet-trace; dotnet tool install --global dotnet-dsrouter. Missing: $($missingTools -join ', ')."
    }
}

function Select-ProfilePlatform {
    while ($true) {
        Write-Host "`n=== Choose a profiling target ===" -ForegroundColor Cyan

        if ($IsWindows) {
            Write-Host '  1. Android device or emulator' -ForegroundColor White
            if ((Read-Host 'Select a target') -eq '1') {
                return 'Android'
            }
            Write-Host 'Choose 1 for Android.' -ForegroundColor Yellow
        }
        elseif ($IsMacOS) {
            Write-Host '  1. iOS simulator' -ForegroundColor White
            Write-Host '  2. Android device or emulator' -ForegroundColor White
            switch (Read-Host 'Select a target') {
                '1' { return 'iOS' }
                '2' { return 'Android' }
                default { Write-Host 'Choose 1 for iOS or 2 for Android.' -ForegroundColor Yellow }
            }
        }
        else {
            Write-Host '  1. Android device or emulator' -ForegroundColor White
            if ((Read-Host 'Select a target') -eq '1') {
                return 'Android'
            }
            Write-Host 'Choose 1 for Android.' -ForegroundColor Yellow
        }
    }
}

if ($Platform -eq 'Auto') {
    $Platform = Select-ProfilePlatform
}
$framework = switch ($Platform) {
    'Android' { 'net10.0-android' }
    'iOS' { 'net10.0-ios' }
}
$output = Join-Path $outputDirectory "simpleapp-startup-$($Platform.ToLower()).speedscope.json"

Show-DemoHeader `
    -Title '09. Profile app startup' `
    -Why 'A startup trace makes launch-time CPU and timing costs visible instead of anecdotal.' `
    -What "Launch and trace SimpleApp on $Platform for $Duration, then write a Speedscope-compatible trace."

Invoke-DemoCommand `
    -Command 'Get-Command dotnet-trace, dotnet-dsrouter' `
    -Does 'Verifies the diagnostics tools DevFlow uses instead of its Windows dnx fallback.' `
    -Run { Assert-ProfilingTools }

Invoke-DemoCommand `
    -Command "New-Item -ItemType Directory -Path `"$outputDirectory`" -Force" `
    -Does 'Creates the local folder that will hold the trace output.' `
    -Run { New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null }

$arguments = @('profile', 'startup', '--project', $project, '--framework', $framework, '--configuration', 'Release', '--duration', $Duration, '--format', 'speedscope', '--stopping-event-provider-name', 'Microsoft.Maui.ProfilingHelper', '--stopping-event-event-name', 'StartupComplete', '--output', $output)
if (-not [string]::IsNullOrWhiteSpace($Device)) {
    $arguments += '--device', $Device
}

Invoke-DemoCommand `
    -Command "maui $($arguments -join ' ')" `
    -Does 'Builds, launches, and collects a bounded startup trace from SimpleApp.' `
    -Run { & maui @arguments }