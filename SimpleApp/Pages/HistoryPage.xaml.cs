using SimpleApp.Core;

namespace SimpleApp.Pages;

public partial class HistoryPage : ContentPage
{
	readonly CalculatorService _calculator;

	public HistoryPage()
	{
		InitializeComponent();
		_calculator = ResolveCalculator();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		HistoryList.ItemsSource = _calculator.History
			.OrderByDescending(h => h.Timestamp)
			.ToList();
	}

	void OnClearHistoryClicked(object? sender, EventArgs e)
	{
		_calculator.ClearHistory();
		HistoryList.ItemsSource = Array.Empty<CalculationHistory>();
	}

	async void OnBackClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//MainPage");
	}

	static CalculatorService ResolveCalculator()
	{
		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("MAUI services are not available.");
		return services.GetRequiredService<CalculatorService>();
	}
}
