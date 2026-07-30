$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')
$project = Join-Path $PSScriptRoot '..\..\SimpleApp\SimpleApp.csproj'

Show-DemoHeader `
	-Title '02. Inspect the project MAUI version' `
	-Why 'A MAUI app can pin package versions, so the project should make its effective MAUI version observable.' `
	-What 'Show the MAUI version and its source for the existing SimpleApp project.'

Invoke-DemoCommand `
	-Command "maui project version show --project `"$project`"" `
	-Does 'Reports the effective MAUI package version before the app is built or launched.' `
	-Run { maui project version show --project $project }