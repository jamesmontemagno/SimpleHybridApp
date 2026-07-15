function Show-DemoHeader {
    param(
        [string]$Title,
        [string]$Why,
        [string]$What
    )

    Write-Host "`n=== $Title ===" -ForegroundColor Cyan
    Write-Host "Why:  $Why" -ForegroundColor Yellow
    Write-Host "What: $What" -ForegroundColor Gray
}

function Invoke-DemoCommand {
    param(
        [string]$Command,
        [string]$Does,
        [scriptblock]$Run
    )

    Write-Host "`nCommand: $Command" -ForegroundColor Green
    Write-Host "Does:    $Does" -ForegroundColor DarkGray
    & $Run
}