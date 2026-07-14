using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleApp.Services;

namespace SimpleApp.ViewModels;

public sealed partial class MathBuddySettingsViewModel(MathBuddySettingsService settingsService) : ObservableObject
{
	[ObservableProperty]
	public partial string ApiKey { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Endpoint { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string Model { get; set; } = "gpt-4o-mini";

	[ObservableProperty]
	public partial string StatusText { get; set; } = string.Empty;

	public void Load()
	{
		var settings = settingsService.GetSettings();
		ApiKey = settings.ApiKey;
		Endpoint = settings.Endpoint;
		Model = settings.ModelOrDefault;
		StatusText = settings.HasCloudConfiguration
			? $"Loaded from {settings.Source}."
			: "No cloud settings saved.";
	}

	[RelayCommand]
	async Task SaveAsync()
	{
		await settingsService.SaveAsync(new MathBuddySettings
		{
			ApiKey = ApiKey,
			Endpoint = Endpoint,
			Model = Model
		});

		StatusText = "Settings saved.";
	}

	[RelayCommand]
	async Task ClearSettingsAsync()
	{
		await settingsService.ClearAsync();
		Load();
		StatusText = "Saved app settings cleared.";
	}
}