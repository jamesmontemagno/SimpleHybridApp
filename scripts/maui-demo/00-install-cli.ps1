$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'demo-helpers.ps1')

Show-DemoHeader `
    -Title '00. Install the MAUI CLI' `
    -Why 'The MAUI CLI provides environment diagnostics, device discovery, and DevFlow commands.' `
    -What 'Install or update the experimental Microsoft.Maui.Cli global tool, then show its version.'

if (Get-Command maui -ErrorAction SilentlyContinue) {
    Invoke-DemoCommand `
        -Command 'dotnet tool update -g Microsoft.Maui.Cli --prerelease' `
        -Does 'Updates the experimental MAUI CLI global tool.' `
        -Run { dotnet tool update -g Microsoft.Maui.Cli --prerelease }
}
else {
    Invoke-DemoCommand `
        -Command 'dotnet tool install -g Microsoft.Maui.Cli --prerelease' `
        -Does 'Installs the experimental MAUI CLI global tool.' `
        -Run { dotnet tool install -g Microsoft.Maui.Cli --prerelease }
}

Invoke-DemoCommand `
    -Command 'maui version' `
    -Does 'Shows the installed MAUI CLI version and runtime information.' `
    -Run { maui version }