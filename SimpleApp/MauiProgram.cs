using Microsoft.Extensions.Logging;
using SimpleApp.Core;
using SimpleApp.Services;
using SimpleApp.ViewModels;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace SimpleApp;

public static class MauiProgram
{
	static int ResolveAgentPort()
		=> int.TryParse(Environment.GetEnvironmentVariable("DEVFLOW_TEST_PORT"), out var envPort)
			? envPort
			: 9223;

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<CalculatorService>();
		builder.Services.AddSingleton<MathBuddySettingsService>();
		builder.Services.AddSingleton<MathBuddyClientFactory>();
		builder.Services.AddSingleton<MathBuddyTools>();
		builder.Services.AddSingleton<MathBuddyService>();
		builder.Services.AddSingleton<MathBuddyViewModel>();
		builder.Services.AddSingleton<MathBuddySettingsViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

#if MAUI_DEVFLOW
		builder.AddMauiDevFlowAgent(options =>
		{
			options.Port = ResolveAgentPort();
		});
#endif

		return builder.Build();
	}

}
