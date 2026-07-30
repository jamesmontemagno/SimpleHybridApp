using System.ComponentModel;
using Microsoft.Maui.AI.Attributes;
using SimpleApp.Core;

namespace SimpleApp.Services;

/// <summary>
/// Calculator-aware tools the Math Buddy assistant can call.
/// </summary>
public sealed class MathBuddyTools(CalculatorService calculator)
{
	[ExportAIFunction("get_calculator_state")]
	[Description("Gets the calculator display, expression, and recent history for tutoring context.")]
	public string GetCalculatorState(
		[Description("Maximum number of recent history items to include")] int maxHistory = 5)
	{
		maxHistory = Math.Clamp(maxHistory, 0, 20);
		var history = calculator.History
			.TakeLast(maxHistory)
			.Select(h => $"{h.Expression} = {h.ResultDisplay}")
			.ToArray();

		return $$"""
			Display: {{calculator.Display}}
			Expression: {{(string.IsNullOrWhiteSpace(calculator.Expression) ? "(none)" : calculator.Expression)}}
			HistoryCount: {{calculator.History.Count}}
			RecentHistory:
			{{(history.Length == 0 ? "(empty)" : string.Join(Environment.NewLine, history.Select(x => "- " + x)))}}
			""";
	}

	[ExportAIFunction("explain_current_calculation")]
	[Description("Builds a concise tutoring prompt for the current calculator expression/result.")]
	public string ExplainCurrentCalculation()
	{
		if (string.IsNullOrWhiteSpace(calculator.Display) || calculator.Display is "0" or "Error")
			return "There is no meaningful calculation on the display yet.";

		var expression = string.IsNullOrWhiteSpace(calculator.Expression)
			? calculator.Display
			: $"{calculator.Expression} → {calculator.Display}";

		return $"Current calculation context: {expression}. Explain step by step for a learner.";
	}

	[ExportAIFunction("list_recent_history")]
	[Description("Lists recent calculator history entries.")]
	public string ListRecentHistory(
		[Description("Maximum entries to return")] int count = 5)
	{
		count = Math.Clamp(count, 1, 20);
		if (calculator.History.Count == 0)
			return "No calculations in history yet.";

		return string.Join(
			Environment.NewLine,
			calculator.History.TakeLast(count).Select((h, i) => $"{i + 1}. {h.Expression} = {h.ResultDisplay}"));
	}
}

/// <summary>
/// Source-generated AI tool context for Math Buddy.
/// </summary>
[AIToolSource(typeof(MathBuddyTools))]
public partial class MathBuddyToolContext : AIToolContext;
