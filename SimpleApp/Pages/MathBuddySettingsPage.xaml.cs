using SimpleApp.ViewModels;

namespace SimpleApp.Pages;

public partial class MathBuddySettingsPage : ContentPage
{
	readonly MathBuddySettingsViewModel _vm;

	public MathBuddySettingsPage()
	{
		InitializeComponent();

		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("MAUI services are not available.");
		_vm = services.GetRequiredService<MathBuddySettingsViewModel>();
		BindingContext = _vm;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_vm.Load();
	}
}