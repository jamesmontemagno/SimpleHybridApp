using System.ComponentModel;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent.Core;
#endif
using SimpleApp.Core;

namespace SimpleApp;

/// <summary>
/// Discoverable DevFlow shortcuts for UI automation and MSTest integration tests.
/// See: https://github.com/dotnet/maui-labs
/// </summary>
public static class DevFlowActions
{
#if MAUI_DEVFLOW
	[DevFlowAction("clear-calculator", Description = "Reset the calculator display and pending operation")]
	public static void ClearCalculator()
	{
		var calculator = GetCalculator();
		MainThread.BeginInvokeOnMainThread(calculator.Clear);
	}

	[DevFlowAction("clear-history", Description = "Clear calculation history")]
	public static void ClearHistory()
	{
		var calculator = GetCalculator();
		MainThread.BeginInvokeOnMainThread(calculator.ClearHistory);
	}

	[DevFlowAction("seed-calculation", Description = "Seed a history entry for the given expression")]
	public static string SeedCalculation(
		[Description("Left operand")] double left = 20,
		[Description("Operator: + - * /")] string op = "+",
		[Description("Right operand")] double right = 5)
	{
		var calculator = GetCalculator();
		var result = op switch
		{
			"+" => left + right,
			"-" => left - right,
			"*" => left * right,
			"/" => right == 0 ? double.NaN : left / right,
			_ => throw new ArgumentException($"Unsupported operator '{op}'", nameof(op))
		};

		MainThread.BeginInvokeOnMainThread(() =>
		{
			calculator.Clear();
			calculator.AddToHistory($"{left} {op} {right}", result);
		});

		return result.ToString("0.##########");
	}

	[DevFlowAction("navigate-history", Description = "Navigate to the history page")]
	public static Task NavigateHistory()
		=> MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//HistoryPage"));

	[DevFlowAction("navigate-calculator", Description = "Navigate to the calculator page")]
	public static Task NavigateCalculator()
		=> MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//MainPage"));

	[DevFlowAction("get-display", Description = "Return the current calculator display text")]
	public static string GetDisplay()
		=> GetCalculator().Display;

	static CalculatorService GetCalculator()
	{
		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("MAUI services are not available.");
		return services.GetRequiredService<CalculatorService>();
	}
#endif
}
