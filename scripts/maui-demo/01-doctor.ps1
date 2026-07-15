[CmdletBinding()]
param(
	[ValidateSet('Doctor', 'AndroidSdkCheck', 'AndroidJdkCheck', 'AppleXcode')]
	[string]$Action
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')

function Test-MacOS {
	if (-not $IsMacOS) {
		Write-Host 'Apple inspection actions can only run on macOS. Choose another menu option.' -ForegroundColor Yellow
		return $false
	}

	return $true
}

function Get-DoctorMenuAction {
	while ($true) {
		Write-Host "`n=== 01. MAUI environment menu ===" -ForegroundColor Cyan
		Write-Host 'Choose a command to show during the demo. Enter 0 to exit.' -ForegroundColor Gray
		Write-Host '  1. Run maui doctor' -ForegroundColor White
		Write-Host '  2. Check Android SDK' -ForegroundColor White
		Write-Host '  3. Check Android JDK' -ForegroundColor White
		Write-Host '  4. List Apple Xcode installations (macOS)' -ForegroundColor White
		Write-Host '  0. Exit this menu' -ForegroundColor White

		switch (Read-Host 'Select an option') {
			'1' { return 'Doctor' }
			'2' { return 'AndroidSdkCheck' }
			'3' { return 'AndroidJdkCheck' }
			'4' { return 'AppleXcode' }
			'0' { return $null }
			default { Write-Host 'Choose a number from 0 through 4.' -ForegroundColor Yellow }
		}
	}
}

function Invoke-DoctorAction {
	param([string]$SelectedAction)

	switch ($SelectedAction) {
		'Doctor' {
			Show-DemoHeader `
				-Title '01. Diagnose the MAUI environment' `
				-Why 'A live MAUI demo is more reliable when the local SDKs and workloads are verified first.' `
				-What 'Run MAUI CLI diagnostics for the current development environment.'
			Invoke-DemoCommand -Command 'maui doctor' -Does 'Checks the installed SDKs, workloads, and platform prerequisites.' -Run { maui doctor }
		}
		'AndroidSdkCheck' {
			Show-DemoHeader `
				-Title '01. Check the Android SDK' `
				-Why 'The Android SDK provides the platform tools needed to build and deploy a MAUI Android app.' `
				-What 'Verify the configured Android SDK installation.'
			Invoke-DemoCommand -Command 'maui android sdk check' -Does 'Checks the Android SDK path and required tooling.' -Run { maui android sdk check }
		}
		'AndroidJdkCheck' {
			Show-DemoHeader `
				-Title '01. Check the Android JDK' `
				-Why 'Android builds require a compatible Java Development Kit.' `
				-What 'Verify the JDK used by Android tooling.'
			Invoke-DemoCommand -Command 'maui android jdk check' -Does 'Checks the configured Java Development Kit.' -Run { maui android jdk check }
		}
		'AppleXcode' {
			if (-not (Test-MacOS)) { return }
			Show-DemoHeader `
				-Title '01. List Apple Xcode installations' `
				-Why 'Xcode supplies the Apple SDKs and build tools required for MAUI Apple targets.' `
				-What 'List the Xcode installations available on this Mac.'
			Invoke-DemoCommand -Command 'maui apple xcode list' -Does 'Lists installed Xcode versions.' -Run { maui apple xcode list }
		}
	}
}

if ($PSBoundParameters.ContainsKey('Action')) {
	Invoke-DoctorAction -SelectedAction $Action
	return
}

while ($true) {
	$selectedAction = Get-DoctorMenuAction
	if ($null -eq $selectedAction) {
		break
	}

	Invoke-DoctorAction -SelectedAction $selectedAction
	Read-Host "`nPress Enter to return to the environment menu" | Out-Null
}