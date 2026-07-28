using Microsoft.Extensions.DependencyInjection;

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

		return window;
	}
}