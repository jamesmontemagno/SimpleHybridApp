using SimpleApp.Core;
using SimpleApp.Services;

namespace SimpleApp;

public partial class MainPage : ContentPage
{
	readonly CalculatorService _calculator;

	public MainPage()
	{
		InitializeComponent();
		_calculator = ResolveCalculator();
		RefreshUi();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshUi();
	}

	void OnDigitClicked(object? sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		_calculator.InputDigit(button.Text);
		RefreshUi();
	}

	void OnOperatorClicked(object? sender, EventArgs e)
	{
		if (sender is not Button button || string.IsNullOrEmpty(button.Text))
			return;

		_calculator.InputOperator(button.Text[0]);
		RefreshUi("Operator set");
	}

	async void OnEqualsClicked(object? sender, EventArgs e)
	{
		var result = _calculator.Calculate();
		RefreshUi(double.IsNaN(result) ? "Division by zero" : $"Result: {_calculator.Display}");

		if (_calculator.ShouldCelebrate)
			await CelebrateAsync();
	}

	void OnClearClicked(object? sender, EventArgs e)
	{
		_calculator.Clear();
		RefreshUi("Cleared");
	}

	async void OnHistoryClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//HistoryPage");
	}

	async void OnMathBuddyClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//MathBuddyPage");
	}

	async void OnExplainClicked(object? sender, EventArgs e)
	{
		var services = IPlatformApplication.Current?.Services;
		var mathBuddy = services?.GetService<MathBuddyService>();
		if (mathBuddy is null)
		{
			StatusLabel.Text = "Math Buddy service unavailable";
			return;
		}

		if (!mathBuddy.IsAvailable)
		{
			StatusLabel.Text = "AI not configured";
			await DisplayAlertAsync("Math Buddy", mathBuddy.AvailabilityMessage, "OK");
			return;
		}

		StatusLabel.Text = "Asking Math Buddy...";
		try
		{
			var explanation = await mathBuddy.ExplainCurrentCalculationAsync();
			await DisplayAlertAsync("Math Buddy", explanation, "OK");
			StatusLabel.Text = "Insight ready";
		}
		catch (Exception ex)
		{
			StatusLabel.Text = "Explain failed";
			await DisplayAlertAsync("Math Buddy", ex.Message, "OK");
		}
	}

	async Task CelebrateAsync()
	{
		var celebrate = await DisplayAlertAsync(
			"Celebrate",
			"You reached 25! Do you want to celebrate?",
			"Yes",
			"No");

		StatusLabel.Text = celebrate
			? "Celebration time! 🎉"
			: "Maybe next time";
	}

	void RefreshUi(string? status = null)
	{
		DisplayLabel.Text = _calculator.Display;
		ExpressionLabel.Text = _calculator.Expression;
		if (status is not null)
			StatusLabel.Text = status;
	}

	static CalculatorService ResolveCalculator()
	{
		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("MAUI services are not available.");
		return services.GetRequiredService<CalculatorService>();
	}
}
