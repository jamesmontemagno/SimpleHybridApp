# SimpleHybridApp Samples

Sample apps that implement the same calculator experience three ways, plus DevFlow-powered MSTest coverage for the native MAUI app and a Math Buddy AI tutor in `SimpleApp`.

## Projects

| Project | Description |
|---------|-------------|
| `SimpleApp` | **Native .NET MAUI** calculator (XAML + C#) + **Math Buddy** tutor |
| `SimpleHybridApp` | HybridWebView calculator (HTML/JS in MAUI) |
| `SimpleBlazorHybridApp` | Blazor Hybrid calculator |
| `SimpleApp.Core` | Shared calculator logic used by SimpleApp + unit tests |
| `SimpleApp.Tests` | MSTest unit tests for calculator logic |
| `SimpleApp.DevFlow.Tests` | MSTest UI tests via [Microsoft.Maui.DevFlow](https://github.com/dotnet/maui-labs) Driver |

## Shared functionality

All three apps implement:

- Basic calculator (`+ - * /`)
- Calculation history
- Celebrate prompt when a result equals **25**

## SimpleApp (native MAUI)

Scaffolded with:

```bash
dotnet new maui -n SimpleApp -f net10.0
```

Key pieces:

- `MainPage` — calculator UI with stable `AutomationId`s for DevFlow
- `Pages/HistoryPage` — history list (`CollectionView`)
- `Pages/MathBuddyPage` — AI tutor chat UI
- `Pages/MathBuddySettingsPage` — cloud AI settings saved outside source control
- `SimpleApp.Core/CalculatorService` — pure calculator logic
- `Services/MathBuddyService` + `MathBuddyTools` — Essentials.AI / OpenAI chat + calculator tools
- `DevFlowActions` — `[DevFlowAction]` shortcuts (`clear-calculator`, `seed-calculation`, etc.)
- DevFlow agent enabled in Debug via `Microsoft.Maui.DevFlow.Agent` (or `-p:MauiDevFlowEnabled=true` with VS Code targets)

### Math Buddy (Essentials.AI)

Math Buddy is a math tutor integrated into the native app:

- **Chat tab** — ask free-form math questions, stream answers
- **Calculator → Explain** — ask for insight on the current expression/display
- **Tools** — source-generated with `Microsoft.Maui.AI.Attributes` (`get_calculator_state`, `explain_current_calculation`, `list_recent_history`)

Backends:

| Platform | Provider |
|----------|----------|
| iOS / Mac Catalyst 26+ | On-device [Microsoft.Maui.Essentials.AI](https://github.com/dotnet/maui-labs#essentialsai) (`AppleIntelligenceChatClient`) |
| Windows / Android / older Apple | Optional OpenAI-compatible cloud via Settings |

Cloud configuration (when Apple Intelligence is not available) can be entered in the app's Settings tab.

For Debug builds only, the app also reads `SimpleApp/mathbuddy.local.json` when it is present. This file is ignored by Git and copied to the app output only for Debug builds:

```json
{
	"apiKey": "<key>",
	"endpoint": "https://<resource>.openai.azure.com/",
	"model": "gpt-4o-mini"
}
```

Debug builds also accept environment variables:

```bash
# required
$env:OPENAI_API_KEY = "<key>"

# optional (Azure OpenAI or custom gateway)
$env:OPENAI_ENDPOINT = "https://<resource>.openai.azure.com/"
$env:OPENAI_MODEL = "gpt-4o-mini"   # or your deployment name
```

Without a backend, the UI still loads and shows configuration guidance.

### Run SimpleApp (Windows)

```bash
dotnet build SimpleApp/SimpleApp.csproj -f net10.0-windows10.0.19041.0 -c Debug
# then launch the generated SimpleApp.exe, or use MAUI debug tooling
```

### Unit tests

```bash
dotnet test SimpleApp.Tests/SimpleApp.Tests.csproj
```

### DevFlow UI tests

These tests build/launch SimpleApp on Windows and drive it with `Microsoft.Maui.DevFlow.Driver` (same approach as the integration fixtures in [dotnet/maui-labs](https://github.com/dotnet/maui-labs)).

```bash
dotnet test SimpleApp.DevFlow.Tests/SimpleApp.DevFlow.Tests.csproj
```

Optional env vars:

- `DEVFLOW_TEST_PORT` — agent port (fixture allocates one automatically)

## SimpleHybridApp

HybridWebView calculator with confetti celebration and in-page history.

## SimpleBlazorHybridApp

Blazor Hybrid calculator with `/calculator` and `/history` routes.

## Requirements

- .NET 10 SDK
- .NET MAUI workload (`maui-windows`, `android`, etc. as needed)
- For DevFlow tests: Windows machine and DevFlow agent package
- For Math Buddy cloud mode: `OPENAI_API_KEY` (optional endpoint/model)
- For Math Buddy on-device Apple mode: iOS/Mac Catalyst 26+ with Apple Intelligence