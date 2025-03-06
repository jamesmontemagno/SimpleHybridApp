namespace SimpleBlazorHybridApp.Services;

public class CalculatorService
{
    private List<CalculationHistory> _history = new();

    public IReadOnlyList<CalculationHistory> History => _history.AsReadOnly();

    public void AddToHistory(string expression, double result)
    {
        _history.Add(new CalculationHistory
        {
            Expression = expression,
            Result = result,
            Timestamp = DateTime.Now
        });
    }

    public void ClearHistory()
    {
        _history.Clear();
    }
}

public class CalculationHistory
{
    public string Expression { get; set; } = "";
    public double Result { get; set; }
    public DateTime Timestamp { get; set; }
}
