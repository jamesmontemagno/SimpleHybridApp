$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')

Show-DemoHeader `
	-Title '06. Automate a calculator workflow' `
	-Why 'Stable AutomationId values allow reliable automation, assertions, and screenshots across UI layout changes.' `
	-What 'Calculate 2 + 3, assert that the live display is 5, and capture a screenshot.'

Invoke-DemoCommand -Command 'maui devflow ui tap --automationId BtnClear' -Does 'Resets the calculator state.' -Run { maui devflow ui tap --automationId 'BtnClear' }
Invoke-DemoCommand -Command 'maui devflow ui tap --automationId Btn2' -Does 'Taps the calculator digit 2.' -Run { maui devflow ui tap --automationId 'Btn2' }
Invoke-DemoCommand -Command 'maui devflow ui tap --automationId BtnAdd' -Does 'Taps the addition operator.' -Run { maui devflow ui tap --automationId 'BtnAdd' }
Invoke-DemoCommand -Command 'maui devflow ui tap --automationId Btn3' -Does 'Taps the calculator digit 3.' -Run { maui devflow ui tap --automationId 'Btn3' }
Invoke-DemoCommand -Command 'maui devflow ui tap --automationId BtnEquals' -Does 'Evaluates the current expression.' -Run { maui devflow ui tap --automationId 'BtnEquals' }
Invoke-DemoCommand -Command 'maui devflow ui assert --automationId DisplayLabel Text 5' -Does 'Verifies that the live calculator display reports 5.' -Run { maui devflow ui assert --automationId 'DisplayLabel' Text 5 }
Invoke-DemoCommand -Command 'maui devflow ui screenshot --output simpleapp-2-plus-3.png --overwrite' -Does 'Saves visual evidence of the completed calculation.' -Run { maui devflow ui screenshot --output simpleapp-2-plus-3.png --overwrite }