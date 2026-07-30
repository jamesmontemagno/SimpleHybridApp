[CmdletBinding()]
param(
    [ValidateSet('List', 'CreateAndroidEmulator', 'StartAndroidEmulator', 'CreateAppleSimulator', 'StartAppleSimulator')]
    [string]$Action,
    [string]$Name,
    [string]$NameOrUdid,
    [string]$Package,
    [string]$Device,
    [string]$DeviceType,
    [string]$Runtime,
    [switch]$ColdBoot
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')

function Test-MacOS {
    if (-not $IsMacOS) {
        Write-Host 'Apple simulator actions can only run on macOS. Choose another menu option.' -ForegroundColor Yellow
        return $false
    }

    return $true
}

function Get-DeviceMenuAction {
    while ($true) {
        Write-Host "`n=== 03. Device management menu ===" -ForegroundColor Cyan
        Write-Host 'Choose a command to show during the demo. Enter 0 to exit.' -ForegroundColor Gray
        Write-Host '  1. List connected devices' -ForegroundColor White
        Write-Host '  2. Create an Android emulator' -ForegroundColor White
        Write-Host '  3. Start an Android emulator' -ForegroundColor White
        Write-Host '  4. Create an Apple simulator (macOS)' -ForegroundColor White
        Write-Host '  5. Start an Apple simulator (macOS)' -ForegroundColor White
        Write-Host '  0. Exit this menu' -ForegroundColor White

        switch (Read-Host 'Select an option') {
            '1' { return 'List' }
            '2' { return 'CreateAndroidEmulator' }
            '3' { return 'StartAndroidEmulator' }
            '4' { return 'CreateAppleSimulator' }
            '5' { return 'StartAppleSimulator' }
            '0' { return $null }
            default { Write-Host 'Choose a number from 0 through 5.' -ForegroundColor Yellow }
        }
    }
}

function Invoke-DeviceAction {
    param([string]$SelectedAction)

switch ($SelectedAction) {
    'List' {
        Show-DemoHeader `
            -Title '03. List available devices' `
            -Why 'MAUI targets physical devices, emulators, and simulators across platforms.' `
            -What 'List the targets the MAUI CLI can discover before launching the app.'
        Invoke-DemoCommand `
            -Command 'maui device list' `
            -Does 'Lists connected physical devices, running emulators, and booted simulators.' `
            -Run { maui device list }
    }
    'CreateAndroidEmulator' {
        Show-DemoHeader `
            -Title '03. Create an Android emulator' `
            -Why 'Creating an Android Virtual Device from the CLI makes a mobile demo repeatable on a fresh machine.' `
            -What 'Create a named emulator, optionally choosing an Android system image and device definition.'
        if ([string]::IsNullOrWhiteSpace($Name)) {
            $Name = Read-Host 'New Android emulator name'
        }
        if ([string]::IsNullOrWhiteSpace($Name)) {
            throw 'An Android emulator name is required.'
        }
        $arguments = @('android', 'emulator', 'create', $Name)
        if (-not [string]::IsNullOrWhiteSpace($Package)) {
            $arguments += '--package', $Package
        }
        if (-not [string]::IsNullOrWhiteSpace($Device)) {
            $arguments += '--device', $Device
        }
        Invoke-DemoCommand `
            -Command "maui $($arguments -join ' ')" `
            -Does 'Creates the requested Android Virtual Device.' `
            -Run { & maui @arguments }
    }
    'StartAndroidEmulator' {
        Show-DemoHeader `
            -Title '03. Start an Android emulator' `
            -Why 'An Android emulator provides a repeatable mobile target for the MAUI app and DevFlow demo.' `
            -What 'Start a named Android Virtual Device and wait until it is ready.'
        if ([string]::IsNullOrWhiteSpace($Name)) {
            Invoke-DemoCommand `
                -Command 'maui android emulator list' `
                -Does 'Lists the available emulator names before you choose one.' `
                -Run { maui android emulator list }
            $Name = Read-Host 'Android emulator name'
        }
        if ([string]::IsNullOrWhiteSpace($Name)) {
            throw 'An Android emulator name is required.'
        }
        $arguments = @('android', 'emulator', 'start', $Name, '--wait')
        if ($ColdBoot) {
            $arguments += '--cold-boot'
        }
        Invoke-DemoCommand `
            -Command "maui $($arguments -join ' ')" `
            -Does 'Starts the emulator and waits for Android to finish booting.' `
            -Run { & maui @arguments }
    }
    'CreateAppleSimulator' {
        if (-not (Test-MacOS)) { return }
        Show-DemoHeader `
            -Title '03. Create an Apple simulator' `
            -Why 'Simulator provisioning can be scripted so every Apple MAUI demo starts with a known target.' `
            -What 'Create an Apple simulator using a device type and, optionally, a specific runtime.'
        if ([string]::IsNullOrWhiteSpace($DeviceType)) {
            $DeviceType = Read-Host 'Device type identifier, for example com.apple.CoreSimulator.SimDeviceType.iPhone-16'
        }
        if ([string]::IsNullOrWhiteSpace($DeviceType)) {
            throw 'An Apple simulator device type identifier is required.'
        }
        $arguments = @('apple', 'simulator', 'create', $DeviceType)
        if (-not [string]::IsNullOrWhiteSpace($Runtime)) {
            $arguments += '--runtime', $Runtime
        }
        if (-not [string]::IsNullOrWhiteSpace($Name)) {
            $arguments += '--name', $Name
        }
        Invoke-DemoCommand `
            -Command "maui $($arguments -join ' ')" `
            -Does 'Creates the requested Apple simulator.' `
            -Run { & maui @arguments }
    }
    'StartAppleSimulator' {
        if (-not (Test-MacOS)) { return }
        Show-DemoHeader `
            -Title '03. Start an Apple simulator' `
            -Why 'An Apple simulator provides a repeatable macOS target for the MAUI app and DevFlow demo.' `
            -What 'Boot a named Apple simulator or UDID and open the Simulator UI.'
        if ([string]::IsNullOrWhiteSpace($NameOrUdid)) {
            Invoke-DemoCommand `
                -Command 'maui apple simulator list' `
                -Does 'Lists simulator names and UDIDs before you choose one.' `
                -Run { maui apple simulator list }
            $NameOrUdid = Read-Host 'Apple simulator name or UDID'
        }
        if ([string]::IsNullOrWhiteSpace($NameOrUdid)) {
            throw 'An Apple simulator name or UDID is required.'
        }
        Invoke-DemoCommand `
            -Command "maui apple simulator start `"$NameOrUdid`"" `
            -Does 'Boots the selected simulator and opens the Simulator UI.' `
            -Run { maui apple simulator start $NameOrUdid }
    }
}

}

if ($PSBoundParameters.ContainsKey('Action')) {
    Invoke-DeviceAction -SelectedAction $Action
    return
}

while ($true) {
    $selectedAction = Get-DeviceMenuAction
    if ($null -eq $selectedAction) {
        break
    }

    Invoke-DeviceAction -SelectedAction $selectedAction
    Read-Host "`nPress Enter to return to the device management menu" | Out-Null
}