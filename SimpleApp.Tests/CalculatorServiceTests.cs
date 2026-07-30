using SimpleApp.Core;

namespace SimpleApp.Tests;

[TestClass]
public sealed class CalculatorServiceTests
{
	[TestMethod]
	public void Calculate_Add_ReturnsSumAndHistory()
	{
		var calculator = new CalculatorService();

		calculator.InputDigit("2");
		calculator.InputDigit("0");
		calculator.InputOperator('+');
		calculator.InputDigit("5");
		var result = calculator.Calculate();

		Assert.AreEqual(25d, result);
		Assert.AreEqual("25", calculator.Display);
		Assert.IsTrue(calculator.ShouldCelebrate);
		Assert.HasCount(1, calculator.History);
		Assert.AreEqual("20 + 5", calculator.History[0].Expression);
	}

	[TestMethod]
	public void Calculate_Multiply_ReturnsProduct()
	{
		var calculator = new CalculatorService();

		calculator.InputDigit("5");
		calculator.InputOperator('*');
		calculator.InputDigit("5");
		var result = calculator.Calculate();

		Assert.AreEqual(25d, result);
		Assert.IsTrue(calculator.ShouldCelebrate);
	}

	[TestMethod]
	public void Calculate_DivideByZero_ShowsError()
	{
		var calculator = new CalculatorService();

		calculator.InputDigit("8");
		calculator.InputOperator('/');
		calculator.InputDigit("0");
		var result = calculator.Calculate();

		Assert.IsTrue(double.IsNaN(result));
		Assert.AreEqual("Error", calculator.Display);
		Assert.AreEqual("8 / 0", calculator.Expression);
		Assert.IsFalse(calculator.ShouldCelebrate);
		Assert.HasCount(1, calculator.History);
		Assert.AreEqual("8 / 0", calculator.History[0].Expression);
		Assert.AreEqual("Division by zero is undefined.", calculator.History[0].ErrorMessage);
		Assert.AreEqual("Error: Division by zero is undefined.", calculator.History[0].ResultDisplay);
	}

	[TestMethod]
	public void Clear_ResetsDisplayAndPendingOperation()
	{
		var calculator = new CalculatorService();

		calculator.InputDigit("9");
		calculator.InputOperator('+');
		calculator.InputDigit("1");
		calculator.Clear();

		Assert.AreEqual("0", calculator.Display);
		Assert.AreEqual(string.Empty, calculator.Expression);
		Assert.IsFalse(calculator.ShouldCelebrate);
	}

	[TestMethod]
	public void History_CanBeCleared()
	{
		var calculator = new CalculatorService();

		calculator.InputDigit("1");
		calculator.InputOperator('+');
		calculator.InputDigit("1");
		calculator.Calculate();
		calculator.ClearHistory();

		Assert.IsEmpty(calculator.History);
	}
}
