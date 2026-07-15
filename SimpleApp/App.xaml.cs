using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.StartupProfiling;

namespace SimpleApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell())
		{
			Title = "SimpleApp"
		};

		StartupProfilingMarker.Complete();
		return window;
	}
}