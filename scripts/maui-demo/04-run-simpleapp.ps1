[CmdletBinding()]
param(
    [ValidateSet('Auto', 'Windows', 'Android', 'iOS', 'MacCatalyst')]
    [string]$Platform = 'Auto'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')
$project = Join-Path $PSScriptRoot '..\..\SimpleApp\SimpleApp.csproj'

function Select-LaunchPlatform {
    while ($true) {
        Write-Host "`n=== Choose an app launch target ===" -ForegroundColor Cyan

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
            throw 'This demo launcher supports Windows and macOS. Pass an explicit supported -Platform when running elsewhere.'
        }
    }
}

if ($Platform -eq 'Auto') {
    $Platform = Select-LaunchPlatform
}

$targetFramework = switch ($Platform) {
    'Windows' { 'net10.0-windows10.0.19041.0' }
    'Android' { 'net10.0-android' }
    'iOS' { 'net10.0-ios' }
    'MacCatalyst' { 'net10.0-maccatalyst' }
}

Show-DemoHeader `
    -Title '04. Launch SimpleApp' `
    -Why 'DevFlow communicates with an HTTP agent that runs inside the Debug app process.' `
    -What "Build and launch SimpleApp for $Platform. Keep this terminal open while running scripts 05 through 07 in another terminal."

Invoke-DemoCommand `
    -Command "dotnet build `"$project`" -t:Run -f $targetFramework -c Debug" `
    -Does 'Builds the Debug app, starts it on the selected platform, and enables the in-app DevFlow agent.' `
    -Run { dotnet build $project -t:Run -f $targetFramework -c Debug }