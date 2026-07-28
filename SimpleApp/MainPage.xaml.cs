using Microsoft.Maui.ProfilingHelper;
using SimpleApp.Core;
using SimpleApp.Services;

namespace SimpleApp;

public partial class MainPage : ContentPage
{
	readonly CalculatorService _calculator;
	readonly ConfettiDrawable _confetti = new();
	bool _startupMarkerScheduled;

	public MainPage()
	{
		InitializeComponent();
		_calculator = ResolveCalculator();
		ConfettiView.Drawable = _confetti;
		RefreshUi();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshUi();

		if (!_startupMarkerScheduled)
		{
			_startupMarkerScheduled = true;
			Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1), MauiProfilingMarker.Complete);
		}
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

	void OnEqualsClicked(object? sender, EventArgs e)
	{
		var result = _calculator.Calculate();
		RefreshUi(double.IsNaN(result) ? "Division by zero" : $"Result: {_calculator.Display}");

		if (_calculator.ShouldCelebrate)
			Celebrate();
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

	void Celebrate()
	{
		StatusLabel.Text = "Celebration time!";
		_confetti.Start();
		ConfettiView.IsVisible = true;

		Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
		{
			_confetti.Advance();
			ConfettiView.Invalidate();

			if (_confetti.IsComplete)
			{
				ConfettiView.IsVisible = false;
				return false;
			}

			return true;
		});
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

sealed class ConfettiDrawable : IDrawable
{
	static readonly Color[] Colors =
	[
		Color.FromArgb("#512BD4"),
		Color.FromArgb("#E600A9"),
		Color.FromArgb("#00A89C"),
		Color.FromArgb("#F5A623"),
		Color.FromArgb("#1677FF")
	];

	readonly ConfettiPiece[] _pieces = CreatePieces();
	DateTime _startedAt;

	public bool IsComplete => DateTime.UtcNow - _startedAt >= TimeSpan.FromSeconds(2.5);

	public void Start() => _startedAt = DateTime.UtcNow;

	public void Advance()
	{
	}

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		var elapsed = (float)(DateTime.UtcNow - _startedAt).TotalSeconds;
		var progress = Math.Clamp(elapsed / 2.5f, 0f, 1f);

		foreach (var piece in _pieces)
		{
			var x = piece.StartX * dirtyRect.Width + piece.Drift * progress * dirtyRect.Width;
			var y = (-24f + piece.Delay * 90f) + progress * progress * (dirtyRect.Height + 80f);

			canvas.SaveState();
			canvas.Translate(x, y);
			canvas.Rotate((piece.Rotation + progress * 720f) % 360f);
			canvas.FillColor = Colors[piece.ColorIndex];
			canvas.FillRectangle(-piece.Size / 2f, -piece.Size / 2f, piece.Size, piece.Size * 0.55f);
			canvas.RestoreState();
		}
	}

	static ConfettiPiece[] CreatePieces()
	{
		var random = new Random(25);
		return Enumerable.Range(0, 54)
			.Select(_ => new ConfettiPiece(
				(float)random.NextDouble(),
				(float)(random.NextDouble() - 0.5) * 0.35f,
				(float)random.NextDouble() * 3f,
				random.Next(8, 15),
				random.Next(Colors.Length),
				(float)random.NextDouble() * 0.45f))
			.ToArray();
	}

	record ConfettiPiece(float StartX, float Drift, float Rotation, float Size, int ColorIndex, float Delay);
}
