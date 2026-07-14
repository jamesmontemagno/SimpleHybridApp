using System.Text.Json;

namespace SimpleApp.Services;

public sealed class MathBuddySettingsService
{
	public const string AppSettingsFileName = "mathbuddy-settings.json";
	public const string DebugSettingsFileName = "mathbuddy.local.json";

	static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true
	};

	readonly string _settingsPath = Path.Combine(FileSystem.AppDataDirectory, AppSettingsFileName);

	public event EventHandler? SettingsChanged;

	public string SettingsPath => _settingsPath;

	public MathBuddySettings GetSettings()
	{
		var savedSettings = ReadSettingsFile(_settingsPath);
		if (HasAnyValue(savedSettings))
			return Normalize(savedSettings, "app settings");

#if DEBUG
		var debugSettings = GetDebugSettings();
		if (HasAnyValue(debugSettings))
			return debugSettings;
#endif

		return new MathBuddySettings
		{
			Model = "gpt-4o-mini",
			Source = "not configured"
		};
	}

	public async Task SaveAsync(MathBuddySettings settings, CancellationToken cancellationToken = default)
	{
		var normalized = Normalize(settings, "app settings");
		Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

		await using var stream = File.Create(_settingsPath);
		await JsonSerializer.SerializeAsync(stream, normalized, JsonOptions, cancellationToken);

		SettingsChanged?.Invoke(this, EventArgs.Empty);
	}

	public Task ClearAsync()
	{
		if (File.Exists(_settingsPath))
			File.Delete(_settingsPath);

		SettingsChanged?.Invoke(this, EventArgs.Empty);
		return Task.CompletedTask;
	}

#if DEBUG
	static MathBuddySettings GetDebugSettings()
	{
		var debugFilePath = Path.Combine(AppContext.BaseDirectory, DebugSettingsFileName);
		var debugFileSettings = ReadSettingsFile(debugFilePath);
		var hasDebugFile = HasAnyValue(debugFileSettings);

		var settings = new MathBuddySettings
		{
			ApiKey = FirstValue(
				debugFileSettings.ApiKey,
				Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
				Environment.GetEnvironmentVariable("AI__ApiKey")),
			Endpoint = FirstValue(
				debugFileSettings.Endpoint,
				Environment.GetEnvironmentVariable("OPENAI_ENDPOINT"),
				Environment.GetEnvironmentVariable("AI__Endpoint")),
			Model = FirstValue(
				debugFileSettings.Model,
				Environment.GetEnvironmentVariable("OPENAI_MODEL"),
				Environment.GetEnvironmentVariable("AI__DeploymentName"),
				"gpt-4o-mini"),
			Source = hasDebugFile ? "debug local settings" : "debug environment"
		};

		return Normalize(settings, settings.Source);
	}
#endif

	static MathBuddySettings ReadSettingsFile(string path)
	{
		try
		{
			if (!File.Exists(path))
				return new MathBuddySettings();

			var json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<MathBuddySettings>(json, JsonOptions) ?? new MathBuddySettings();
		}
		catch
		{
			return new MathBuddySettings();
		}
	}

	static MathBuddySettings Normalize(MathBuddySettings settings, string source)
	{
		return new MathBuddySettings
		{
			ApiKey = settings.ApiKey.Trim(),
			Endpoint = settings.Endpoint.Trim(),
			Model = string.IsNullOrWhiteSpace(settings.Model) ? "gpt-4o-mini" : settings.Model.Trim(),
			Source = source
		};
	}

	static bool HasAnyValue(MathBuddySettings settings)
		=> !string.IsNullOrWhiteSpace(settings.ApiKey)
			|| !string.IsNullOrWhiteSpace(settings.Endpoint)
			|| !string.IsNullOrWhiteSpace(settings.Model);

	static string FirstValue(params string?[] values)
	{
		foreach (var value in values)
		{
			if (!string.IsNullOrWhiteSpace(value))
				return value;
		}

		return string.Empty;
	}
}