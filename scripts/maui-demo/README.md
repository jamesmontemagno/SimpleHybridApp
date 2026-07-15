# MAUI CLI and DevFlow Demo

Run these scripts from the repository root with PowerShell 7 (`pwsh`). Each one prints a stage title, why it matters, what it demonstrates, the exact command it will run, and what that command does before execution.

| Script | What it shows | Why it matters |
| --- | --- | --- |
| `00-install-cli.ps1` | Installs or updates the experimental MAUI CLI. | Provides the `maui` command used by the rest of the demo. |
| `01-doctor.ps1` | Opens a menu for MAUI, Android SDK/JDK, and Xcode environment checks. | Keeps diagnostics separate from device and simulator management. |
| `02-project-version.ps1` | Shows the MAUI version used by SimpleApp. | Shows project-level MAUI version inspection before building or launching the app. |
| `03-devices.ps1` | Opens a menu for devices, emulators, and simulators. | Lets you choose a target-management command and return to the menu after it runs. |
| `04-run-simpleapp.ps1` | Prompts for a target, then builds and launches SimpleApp in Debug. | Starts the in-app DevFlow agent on the selected platform. |
| `05-discover-agent.ps1` | Guides discovery, tree inspection, element interaction, and a screenshot. | Runs one narrated DevFlow command at a time as you advance the demo. |
| `06-calculate.ps1` | Automates `2 + 3 = 5` and saves a screenshot. | Shows stable AutomationId-based UI automation and assertion. |
| `07-history.ps1` | Opens and inspects calculation History. | Shows an observable multi-screen user journey. |
| `08-start-mcp.ps1` | Starts the DevFlow MCP server. | Makes the same running-app capabilities available to an AI agent. |
| `09-profile-startup.ps1` | Captures a bounded startup trace. | Shows app launch performance analysis. |
| `10-profile-manual.ps1` | Starts an on-demand performance trace. | Shows profiling a workflow after navigation. |

## Suggested order

Run `00`, then use the looping `01-doctor.ps1` menu for environment checks, and run `02-project-version` before building or launching the app. Next, use the looping `03-devices.ps1` menu to list or start a mobile target. In one terminal, run `04-run-simpleapp.ps1` and leave it running. When the calculator window appears, use a second terminal for `05`, `06`, and `07`. Use `08` only when connecting an MCP-capable AI client. Run `09` or `10` when you want a performance trace.

Script `05` lists its full walkthrough plan, shows what comes next at each step, and waits for Enter before each command. Type `Q` instead to stop the guided walkthrough, or pass `-RunAll` to run its full sequence without prompts. Script `08` prints a presenter talk track and suggested AI prompts before starting its foreground MCP server.

## Device Actions

Run `pwsh ./scripts/maui-demo/03-devices.ps1` for the interactive loop. Pass `-Action <name>` to run a single action without the menu.

| Action | Example | Purpose |
| --- | --- | --- |
| `List` | `-Action List` | List connected devices, emulators, and simulators. This is the default. |
| `CreateAndroidEmulator` | `-Action CreateAndroidEmulator -Name DemoApi36` | Create an Android Virtual Device. Optional: `-Package`, `-Device`. |
| `StartAndroidEmulator` | `-Action StartAndroidEmulator -Name MAUI_Emulator_API_36` | Start an Android emulator. Optional: `-ColdBoot`. |
| `CreateAppleSimulator` | `-Action CreateAppleSimulator -DeviceType <id> -Name MauiDemo` | Create an Apple simulator on macOS. Optional: `-Runtime` to select a specific runtime. |
| `StartAppleSimulator` | `-Action StartAppleSimulator -NameOrUdid "iPhone 16 Pro"` | Boot an Apple simulator on macOS. |

The create actions can change your machine. The MAUI CLI supports profiling Android and the iOS simulator only. With no `-Platform` argument, profiling scripts prompt for Android on Windows and iOS simulator or Android on macOS. Pass `-Platform Android` or `-Platform iOS` to skip the prompt. Before profiling, install the required diagnostics tools once: `dotnet tool install --global dotnet-trace` and `dotnet tool install --global dotnet-dsrouter`.

With no `-Platform` argument, script `04` asks where to launch: Windows or Android on Windows; Mac Catalyst, iOS, or Android on macOS. Pass `-Platform Windows`, `-Platform Android`, `-Platform iOS`, or `-Platform MacCatalyst` to skip the prompt.