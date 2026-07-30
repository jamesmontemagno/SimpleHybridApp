$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')

Show-DemoHeader `
	-Title '08. Start the DevFlow MCP server' `
	-Why 'DevFlow exposes its live-app capabilities through MCP so an AI client can inspect and automate the running MAUI app.' `
	-What 'Start the stdio MCP server. Leave this process running while the AI client is connected.'

Write-Host "`n=== Presenter Notes: Say This After Your AI Client Connects ===" -ForegroundColor Cyan
Write-Host 'The server exposes the same live app capabilities through a standard MCP connection.' -ForegroundColor White
Write-Host 'The AI can inspect the real visual tree, interact through AutomationIds, verify state, and capture evidence.' -ForegroundColor White
Write-Host 'It is using structured DevFlow tools against the running app, not guessing from screenshots.' -ForegroundColor White

Write-Host "`nSuggested prompts for the connected AI client:" -ForegroundColor Cyan
Write-Host '  1. Inspect SimpleApp and list the calculator controls currently available.' -ForegroundColor White
Write-Host '  2. Tap Btn2, BtnAdd, Btn3, and BtnEquals, then verify DisplayLabel is 5.' -ForegroundColor White
Write-Host '  3. Navigate to the History page and capture a screenshot.' -ForegroundColor White

Write-Host "`nNote: The MCP server uses standard input/output for its protocol, so these presenter notes are shown before the server starts." -ForegroundColor DarkGray

Invoke-DemoCommand `
	-Command 'maui devflow mcp' `
	-Does 'Starts the MCP server that exposes live MAUI app capabilities to an AI client.' `
	-Run { maui devflow mcp }