[CmdletBinding()]
param(
    [ValidateSet('Auto', 'Android', 'iOS', 'MacCatalyst', 'Windows')]
    [string]$Platform = 'Auto'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')
$project = Join-Path $PSScriptRoot '..\..\SimpleApp\SimpleApp.csproj'
$projectDirectory = Split-Path $project

function Invoke-InProjectDirectory {
    param([scriptblock]$Action)

    Push-Location $projectDirectory
    try {
        & $Action
    }
    finally {
        Pop-Location
    }
}

function Select-LaunchPlatform {
    while ($true) {
        Write-Host "`n=== Choose a launch target ===" -ForegroundColor Cyan

        if ($IsWindows) {
            Write-Host '  1. Windows' -ForegroundColor White
            Write-Host '  2. Android device or emulator' -ForegroundColor White
            switch (Read-Host 'Select a target') {
                '1' { return 'Windows' }
                '2' { return 'Android' }
                default { Write-Host 'Choose 1 for Windows or 2 for Android.' -ForegroundColor Yellow }
            }
        }
        elseif ($IsMacOS) {
            Write-Host '  1. Mac Catalyst' -ForegroundColor White
            Write-Host '  2. iOS simulator' -ForegroundColor White
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
    $Platform = Select-LaunchPlatform
}

$framework = switch ($Platform) {
    'Android' { 'net10.0-android' }
    'iOS' { 'net10.0-ios' }
    'MacCatalyst' { 'net10.0-maccatalyst' }
    'Windows' { 'net10.0-windows10.0.19041.0' }
}

$arguments = @('run', '--project', $project, '-p:Configuration=Debug', '--framework', $framework)
if ($Platform -eq 'iOS') {
    $runtime = if ((& uname -m) -eq 'arm64') { 'iossimulator-arm64' } else { 'iossimulator-x64' }
    $arguments += @('--runtime', $runtime)
}

$appBundle = Join-Path (Split-Path $project) "bin/Debug/$framework/$runtime/SimpleApp.app"

Show-DemoHeader `
    -Title '04. Launch SimpleApp' `
    -Why 'DevFlow communicates with an HTTP agent that runs inside the Debug app process.' `
    -What "Build and launch SimpleApp for $Platform. Keep this terminal open while running scripts 05 through 07 in another terminal."

if ($Platform -eq 'iOS' -and -not (Test-Path $appBundle)) {
    Invoke-DemoCommand `
        -Command "dotnet clean --project `"$project`" -p:Configuration=Debug --framework $framework --runtime $runtime" `
        -Does 'Removes stale iOS simulator build metadata when its expected app bundle is missing.' `
        -Run { Invoke-InProjectDirectory { & dotnet clean --project $project -p:Configuration=Debug --framework $framework --runtime $runtime } }
}

Invoke-DemoCommand `
    -Command "dotnet $($arguments -join ' ')" `
    -Does 'Builds the Debug app, starts it on the selected platform, and enables the in-app DevFlow agent.' `
    -Run { Invoke-InProjectDirectory { & dotnet @arguments } }