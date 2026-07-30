namespace SimpleApp.Core;

public sealed class CalculationHistory
{
	public string Expression { get; init; } = string.Empty;
	public double Result { get; init; }
	public string? ErrorMessage { get; init; }
	public string ResultDisplay => ErrorMessage is null
		? Result.ToString("0.##########")
		: $"Error: {ErrorMessage}";
	public DateTime Timestamp { get; init; } = DateTime.Now;
}
