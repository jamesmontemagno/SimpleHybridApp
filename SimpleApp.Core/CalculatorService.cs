namespace SimpleApp.Core;

/// <summary>
/// Shared calculator logic used by the native UI and unit tests.
/// Mirrors the behavior of the Hybrid and Blazor calculator samples.
/// </summary>
public sealed class CalculatorService
{
	readonly List<CalculationHistory> _history = [];

	public IReadOnlyList<CalculationHistory> History => _history;

	public string Display { get; private set; } = "0";
	public string Expression { get; private set; } = string.Empty;
	public bool ShouldCelebrate { get; private set; }

	string _currentNumber = "0";
	double _lastResult;
	char _lastOperator = '\0';
	bool _newNumber = true;

	public void InputDigit(string digit)
	{
		ShouldCelebrate = false;

		if (digit is not ("." or "0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
			throw new ArgumentException($"Unsupported digit: {digit}", nameof(digit));

		if (digit == ".")
		{
			if (_newNumber)
			{
				_currentNumber = "0.";
				_newNumber = false;
			}
			else if (!_currentNumber.Contains('.'))
			{
				_currentNumber += ".";
			}
		}
		else if (_newNumber || _currentNumber == "0")
		{
			_currentNumber = digit;
			_newNumber = false;
		}
		else
		{
			_currentNumber += digit;
		}

		UpdateDisplay();
	}

	public void InputOperator(char op)
	{
		if (op is not ('+' or '-' or '*' or '/'))
			throw new ArgumentException($"Unsupported operator: {op}", nameof(op));

		ShouldCelebrate = false;

		if (!_newNumber)
			Calculate();

		_lastOperator = op;
		Expression = $"{FormatNumber(_lastResult)} {op}";
		_currentNumber = "0";
		_newNumber = true;
		UpdateDisplay();
	}

	public double Calculate()
	{
		ShouldCelebrate = false;

		if (_lastOperator == '\0')
		{
			_lastResult = ParseNumber(_currentNumber);
			Display = FormatNumber(_lastResult);
			return _lastResult;
		}

		var currentValue = ParseNumber(_currentNumber);
		var fullExpression = $"{Expression} {FormatNumber(currentValue)}";

		if (_lastOperator == '/' && currentValue == 0)
		{
			Clear();
			Display = "Error";
			return double.NaN;
		}

		_lastResult = _lastOperator switch
		{
			'+' => _lastResult + currentValue,
			'-' => _lastResult - currentValue,
			'*' => _lastResult * currentValue,
			'/' => _lastResult / currentValue,
			_ => currentValue
		};

		// Match hybrid sample formatting and avoid noisy floating point tails.
		_lastResult = Math.Round(_lastResult, 10, MidpointRounding.AwayFromZero);

		AddToHistory(fullExpression, _lastResult);

		_currentNumber = FormatNumber(_lastResult);
		Expression = string.Empty;
		_lastOperator = '\0';
		_newNumber = true;
		ShouldCelebrate = Math.Abs(_lastResult - 25d) < 0.0000000001;
		UpdateDisplay();
		return _lastResult;
	}

	public void Clear()
	{
		_currentNumber = "0";
		Expression = string.Empty;
		_lastResult = 0;
		_lastOperator = '\0';
		_newNumber = true;
		ShouldCelebrate = false;
		UpdateDisplay();
	}

	public void ClearHistory() => _history.Clear();

	public void AddToHistory(string expression, double result)
	{
		_history.Add(new CalculationHistory
		{
			Expression = expression,
			Result = result,
			Timestamp = DateTime.Now
		});
	}

	void UpdateDisplay()
	{
		if (string.IsNullOrEmpty(Expression))
			Display = _currentNumber;
		else
			Display = $"{Expression} {_currentNumber}".Trim();
	}

	static double ParseNumber(string value)
		=> double.TryParse(value, out var number) ? number : 0;

	static string FormatNumber(double value)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
			return "Error";

		return value.ToString("0.##########");
	}
}
