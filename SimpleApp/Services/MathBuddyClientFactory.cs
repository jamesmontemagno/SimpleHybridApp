using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
#if IOS || MACCATALYST
using Microsoft.Maui.Essentials.AI;
#endif

namespace SimpleApp.Services;

public sealed class MathBuddyClientFactory
{
	readonly MathBuddySettingsService _settingsService;
#if IOS || MACCATALYST
	readonly IServiceProvider _services;
	readonly ILoggerFactory? _loggerFactory;
#endif

#if IOS || MACCATALYST
	public MathBuddyClientFactory(
		IServiceProvider services,
		MathBuddySettingsService settingsService,
		ILoggerFactory? loggerFactory = null)
	{
		_services = services;
		_settingsService = settingsService;
		_loggerFactory = loggerFactory;
	}
#else
	public MathBuddyClientFactory(MathBuddySettingsService settingsService)
	{
		_settingsService = settingsService;
	}
#endif

	public bool IsAvailable => IsAppleIntelligenceAvailable || _settingsService.GetSettings().HasCloudConfiguration;

	public bool IsUsingAppleIntelligence => IsAppleIntelligenceAvailable;

	public string AvailabilityMessage
	{
		get
		{
			if (IsAppleIntelligenceAvailable)
				return "Math Buddy is ready with Apple Intelligence.";

			var settings = _settingsService.GetSettings();
			if (settings.HasCloudConfiguration)
				return $"Math Buddy is ready with {settings.Source}.";

			return "Math Buddy needs an AI backend. Open Settings to add an API key, or use debug-only environment variables or mathbuddy.local.json while developing.";
		}
	}

	public IChatClient? CreateChatClient()
	{
#if IOS || MACCATALYST
		if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
		{
#pragma warning disable CA1416, MAUIAI0001
			return new AppleIntelligenceChatClient(_loggerFactory, _services);
#pragma warning restore CA1416, MAUIAI0001
		}
#endif

		var settings = _settingsService.GetSettings();
		if (!settings.HasCloudConfiguration)
			return null;

		OpenAIClient client;
		if (!string.IsNullOrWhiteSpace(settings.Endpoint))
		{
			client = new OpenAIClient(
				new ApiKeyCredential(settings.ApiKey),
				new OpenAIClientOptions { Endpoint = new Uri(settings.Endpoint) });
		}
		else
		{
			client = new OpenAIClient(settings.ApiKey);
		}

		return client.GetChatClient(settings.ModelOrDefault).AsIChatClient();
	}

	static bool IsAppleIntelligenceAvailable
	{
		get
		{
#if IOS || MACCATALYST
			return OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);
#else
			return false;
#endif
		}
	}
}