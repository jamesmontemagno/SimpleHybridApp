[CmdletBinding()]
param(
    [ValidateSet('Auto', 'Windows', 'Android', 'iOS', 'MacCatalyst')]
    [string]$Platform = 'Auto',
    [string]$Device
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')
$project = Join-Path $PSScriptRoot '..\..\SimpleApp\SimpleApp.csproj'
$outputDirectory = Join-Path $PSScriptRoot '..\..\artifacts\profiles'

function Select-ProfilePlatform {
    while ($true) {
        Write-Host "`n=== Choose a profiling target ===" -ForegroundColor Cyan

        if ($IsWindows) {
            Write-Host '  1. Windows desktop' -ForegroundColor White
            Write-Host '  2. Android device or emulator' -ForegroundColor White
            switch (Read-Host 'Select a target') {
                '1' { return 'Windows' }
                '2' { return 'Android' }
                default { Write-Host 'Choose 1 for Windows or 2 for Android.' -ForegroundColor Yellow }
            }
        }
        elseif ($IsMacOS) {
            Write-Host '  1. Mac Catalyst' -ForegroundColor White
            Write-Host '  2. iOS device or simulator' -ForegroundColor White
            Write-Host '  3. Android device or emulator' -ForegroundColor White
            switch (Read-Host 'Select a target') {
                '1' { return 'MacCatalyst' }
                '2' { return 'iOS' }
                '3' { return 'Android' }
                default { Write-Host 'Choose 1 for Mac Catalyst, 2 for iOS, or 3 for Android.' -ForegroundColor Yellow }
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
    'Windows' { 'net10.0-windows10.0.19041.0' }
    'Android' { 'net10.0-android' }
    'iOS' { 'net10.0-ios' }
    'MacCatalyst' { 'net10.0-maccatalyst' }
}
$output = Join-Path $outputDirectory "simpleapp-manual-$($Platform.ToLower()).speedscope.json"

Show-DemoHeader `
    -Title '10. Profile a manual workflow' `
    -Why 'Manual profiling captures a specific workflow after you navigate to the interesting screen.' `
    -What "Launch SimpleApp on $Platform, press Enter to start collection, exercise the app, then press Enter again to save a Speedscope trace."

Invoke-DemoCommand `
    -Command "New-Item -ItemType Directory -Path `"$outputDirectory`" -Force" `
    -Does 'Creates the local folder that will hold the trace output.' `
    -Run { New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null }

$arguments = @('profile', 'manual', '--project', $project, '--framework', $framework, '--configuration', 'Release', '--format', 'speedscope', '--output', $output)
if (-not [string]::IsNullOrWhiteSpace($Device)) {
    $arguments += '--device', $Device
}

Invoke-DemoCommand `
    -Command "maui $($arguments -join ' ')" `
    -Does 'Launches SimpleApp and waits for you to begin and end trace collection.' `
    -Run { & maui @arguments }