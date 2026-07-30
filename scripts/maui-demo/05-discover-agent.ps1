[CmdletBinding()]
param([switch]$RunAll)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')
$project = Join-Path $PSScriptRoot '..\..\SimpleApp\SimpleApp.csproj'

Show-DemoHeader `
	-Title '05. Discover and inspect the DevFlow agent' `
	-Why 'DevFlow discovers the app agent and reads the UI at runtime instead of relying on guessed layout details.' `
	-What 'Run one command at a time to discover the app, inspect its tree, interact with one element, and capture a screenshot.'

$steps = @(
	[pscustomobject]@{
		Title = 'Wait for the running app agent'
		Command = 'maui devflow wait --project SimpleApp/SimpleApp.csproj --timeout 30'
		Does = 'Waits for this project to register with the DevFlow broker before inspection begins.'
		Run = { maui devflow wait --project $project --timeout 30 }
	},
	[pscustomobject]@{
		Title = 'List the discovered app'
		Command = 'maui devflow agent list'
		Does = 'Lists the live DevFlow agents available to the CLI.'
		Run = { maui devflow agent list }
	},
	[pscustomobject]@{
		Title = 'Inspect the native visual tree'
		Command = 'maui devflow ui tree --depth 4 --format compact'
		Does = 'Dumps a compact view of the live native MAUI visual tree.'
		Run = { maui devflow ui tree --depth 4 --format compact }
	},
	[pscustomobject]@{
		Title = 'Locate one element by AutomationId'
		Command = 'maui devflow ui query --automationId DisplayLabel --format compact'
		Does = 'Finds the calculator display by its stable AutomationId.'
		Run = { maui devflow ui query --automationId 'DisplayLabel' --format compact }
	},
	[pscustomobject]@{
		Title = 'Interact with one native element'
		Command = 'maui devflow ui tap --automationId Btn5'
		Does = 'Interacts with one native element by tapping the calculator 5 button.'
		Run = { maui devflow ui tap --automationId 'Btn5' }
	},
	[pscustomobject]@{
		Title = 'Capture visual evidence'
		Command = 'maui devflow ui screenshot --output simpleapp-discovery.png --overwrite'
		Does = 'Captures a screenshot of the live native app after the interaction.'
		Run = { maui devflow ui screenshot --output simpleapp-discovery.png --overwrite }
	}
)

Write-Host "`nWalkthrough steps:" -ForegroundColor Cyan
for ($index = 0; $index -lt $steps.Count; $index++) {
	$step = $steps[$index]
	$next = if ($index -lt $steps.Count - 1) { $steps[$index + 1].Title } else { 'Complete the guided discovery walkthrough' }
	Write-Host "  $($index + 1). $($step.Title)" -ForegroundColor White
	Write-Host "     Next: $next" -ForegroundColor DarkGray
}

for ($index = 0; $index -lt $steps.Count; $index++) {
	$step = $steps[$index]
	$next = if ($index -lt $steps.Count - 1) { $steps[$index + 1].Title } else { 'Complete the guided discovery walkthrough' }
	Write-Host "`n--- Step $($index + 1) of $($steps.Count): $($step.Title) ---" -ForegroundColor Cyan
	Write-Host "Next: $next" -ForegroundColor DarkGray

	if (-not $RunAll) {
		$response = Read-Host "`nStep $($index + 1) of $($steps.Count). Press Enter to run the next command, or type Q to stop"
		if ($response -match '^[Qq]$') {
			break
		}
	}

	Invoke-DemoCommand -Command $step.Command -Does $step.Does -Run $step.Run
}