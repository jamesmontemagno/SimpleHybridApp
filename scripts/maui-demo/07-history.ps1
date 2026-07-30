$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')

Show-DemoHeader `
	-Title '07. Inspect calculation history' `
	-Why 'DevFlow can automate a user journey and inspect the UI state it produces.' `
	-What 'Open the calculator History page, confirm its root element, and capture a screenshot.'

Invoke-DemoCommand -Command 'maui devflow ui tap --automationId BtnHistory' -Does 'Navigates through the same History button a user taps.' -Run { maui devflow ui tap --automationId 'BtnHistory' }
Invoke-DemoCommand -Command 'maui devflow ui query --automationId HistoryPage --format compact --wait-until exists --timeout 10' -Does 'Waits for and confirms the History page in the visual tree.' -Run { maui devflow ui query --automationId 'HistoryPage' --format compact --wait-until exists --timeout 10 }
Invoke-DemoCommand -Command 'maui devflow ui screenshot --output simpleapp-history.png --overwrite' -Does 'Saves visual evidence of the History state.' -Run { maui devflow ui screenshot --output simpleapp-history.png --overwrite }