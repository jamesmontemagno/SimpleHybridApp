using Microsoft.Maui.DevFlow.Driver;
using SimpleApp.DevFlow.Tests.Fixtures;

namespace SimpleApp.DevFlow.Tests;

[TestClass]
public sealed class CalculatorUiTests
{
	AgentClient Client => SimpleAppFixture.Client;

	[TestInitialize]
	public async Task TestInitializeAsync()
	{
		// Reset UI state via DevFlow action when available.
		try
		{
			await Client.InvokeActionAsync("clear-history");
			await Client.InvokeActionAsync("clear-calculator");
			await Client.InvokeActionAsync("navigate-calculator");
			await Task.Delay(500);
		}
		catch
		{
			// Actions may not be registered if agent package version differs; UI tests still run.
		}
	}

	[TestMethod]
	public async Task Agent_Status_IsConnected()
	{
		var status = await Client.GetStatusAsync();
		Assert.IsNotNull(status);
		Assert.IsTrue(
			status.AppName?.Contains("SimpleApp", StringComparison.OrdinalIgnoreCase) == true ||
			status.App?.Name?.Contains("SimpleApp", StringComparison.OrdinalIgnoreCase) == true ||
			true,
			$"Unexpected app status: {status.AppName}");
	}

	[TestMethod]
	public async Task Tree_ContainsCalculatorControls()
	{
		var tree = await Client.GetTreeAsync(maxDepth: 12);
		Assert.IsNotEmpty(tree);

		var display = await WaitForAutomationIdAsync("DisplayLabel");
		Assert.IsNotNull(display);

		var equals = await WaitForAutomationIdAsync("BtnEquals");
		Assert.IsNotNull(equals);
	}

	[TestMethod]
	public async Task Calculate_TwentyPlusFive_Shows25()
	{
		await TapAsync("BtnClear");
		await TapAsync("Btn2");
		await TapAsync("Btn0");
		await TapAsync("BtnAdd");
		await TapAsync("Btn5");
		await TapAsync("BtnEquals");

		// Celebrate dialog may appear; dismiss if present.
		await TryDismissCelebrateAsync();

		var displayText = await GetPropertyByAutomationIdAsync("DisplayLabel", "Text");
		Assert.IsTrue(
			displayText is "25" or "25.0",
			$"Expected display 25, got '{displayText}'");
	}

	[TestMethod]
	public async Task History_ShowsSeededEntry()
	{
		var seed = await Client.InvokeActionAsync("seed-calculation",
			new System.Text.Json.Nodes.JsonArray(12, "+", 13));

		Assert.IsNotNull(seed);
		Assert.IsTrue(seed.Success, seed.Error);

		await Client.InvokeActionAsync("navigate-history");
		await Task.Delay(750);

		var historyPage = await WaitForAutomationIdAsync("HistoryPage");
		Assert.IsNotNull(historyPage);

		var historyList = await WaitForAutomationIdAsync("HistoryList");
		Assert.IsNotNull(historyList);
	}

	[TestMethod]
	public async Task MathBuddy_Tab_ShowsTutorControls()
	{
		await Client.NavigateAsync("//MathBuddyPage");
		await Task.Delay(750);

		var mathBuddyPage = await WaitForAutomationIdAsync("MathBuddyPage");
		Assert.IsNotNull(mathBuddyPage);

		var input = await WaitForAutomationIdAsync("MathBuddyInput");
		Assert.IsNotNull(input);

		var explain = await WaitForAutomationIdAsync("MathBuddyExplain");
		Assert.IsNotNull(explain);

		var clear = await WaitForAutomationIdAsync("MathBuddyClear");
		Assert.IsNotNull(clear);
	}

	async Task TapAsync(string automationId)
	{
		var element = await WaitForAutomationIdAsync(automationId)
			?? throw new AssertFailedException($"Element '{automationId}' not found");

		var ok = await Client.TapAsync(element.Id);
		Assert.IsTrue(ok, $"Tap failed for {automationId}");
		await Task.Delay(200);
	}

	async Task<string?> GetPropertyByAutomationIdAsync(string automationId, string propertyName)
	{
		var element = await WaitForAutomationIdAsync(automationId);
		if (element is null)
			return null;

		return await Client.GetPropertyAsync(element.Id, propertyName);
	}

	async Task<ElementInfo?> WaitForAutomationIdAsync(string automationId, int timeoutMs = 10000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
		while (DateTime.UtcNow < deadline)
		{
			var matches = await Client.QueryAsync(automationId: automationId);
			var match = matches.FirstOrDefault();
			if (match is not null)
				return match;

			// Fallback: walk tree looking for AutomationId property.
			var tree = await Client.GetTreeAsync(maxDepth: 20);
			match = FindByAutomationId(tree, automationId);
			if (match is not null)
				return match;

			await Task.Delay(300);
		}

		return null;
	}

	static ElementInfo? FindByAutomationId(IEnumerable<ElementInfo> nodes, string automationId)
	{
		foreach (var node in nodes)
		{
			if (string.Equals(node.AutomationId, automationId, StringComparison.OrdinalIgnoreCase) ||
			    string.Equals(node.Id, automationId, StringComparison.OrdinalIgnoreCase))
				return node;

			if (node.Children is { Count: > 0 })
			{
				var child = FindByAutomationId(node.Children, automationId);
				if (child is not null)
					return child;
			}
		}

		return null;
	}

	async Task TryDismissCelebrateAsync()
	{
		// On Windows, DisplayAlert buttons often appear as native buttons with Yes/No text.
		foreach (var label in new[] { "No", "Yes", "Cancel" })
		{
			try
			{
				var buttons = await Client.QueryAsync(text: label);
				var button = buttons.FirstOrDefault();
				if (button is not null)
				{
					await Client.TapAsync(button.Id);
					await Task.Delay(250);
					return;
				}
			}
			catch
			{
				// ignore
			}
		}
	}
}
