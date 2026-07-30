[CmdletBinding()]
param(
    [ValidateSet('Auto', 'Android', 'iOS', 'MacCatalyst', 'Windows')]
    [string]$Platform = 'Auto',
    [string]$Device
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

function Initialize-AndroidDevFlow {
    $devices = @(& maui device list --json | ConvertFrom-Json | Where-Object {
        $_.platform -eq 'android' -and $_.state -eq 'Connected'
    })

    if ([string]::IsNullOrWhiteSpace($Device)) {
        if ($devices.Count -eq 0) {
            throw 'No connected Android device or emulator was found. Start one with 03-devices.ps1 first.'
        }
        if ($devices.Count -gt 1) {
            throw 'More than one Android target is connected. Re-run with -Device <identifier>.'
        }
        $script:Device = $devices[0].identifier
    }
    elseif ($Device -notin $devices.identifier) {
        throw "Android target '$Device' is not connected. Run 03-devices.ps1 -Action List to see valid identifiers."
    }

    & maui devflow broker start | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "maui devflow broker start exited with code $LASTEXITCODE."
    }

    $broker = & maui devflow broker status --json | ConvertFrom-Json
    $diagnostics = & maui devflow --platform android --device $Device diagnose --json | ConvertFrom-Json
    $adb = $diagnostics.android.adb_path
    if ([string]::IsNullOrWhiteSpace($adb) -or -not (Test-Path $adb)) {
        throw 'DevFlow could not locate adb. Run 01-doctor.ps1 and check the Android environment.'
    }

    Invoke-DemoCommand `
        -Command "adb -s $Device reverse tcp:$($broker.port) tcp:$($broker.port)" `
        -Does 'Lets the Android app register with the DevFlow broker running on the host.' `
        -Run { Invoke-CheckedNativeCommand -Name 'adb reverse' -Run { & $adb -s $Device reverse "tcp:$($broker.port)" "tcp:$($broker.port)" } }

    Invoke-DemoCommand `
        -Command "adb -s $Device forward tcp:9223 tcp:9223" `
        -Does 'Lets DevFlow commands reach the HTTP agent running inside the Android app.' `
        -Run { Invoke-CheckedNativeCommand -Name 'adb forward' -Run { & $adb -s $Device forward 'tcp:9223' 'tcp:9223' } }
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
if ($Platform -eq 'Android') {
    Initialize-AndroidDevFlow
    $arguments += @('--device', $Device)
}
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