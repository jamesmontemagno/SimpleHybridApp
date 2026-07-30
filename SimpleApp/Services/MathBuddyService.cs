using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using SimpleApp.Core;

namespace SimpleApp.Services;

/// <summary>
/// Math tutor built on Microsoft.Extensions.AI + calculator tools.
/// Uses Apple Intelligence (Essentials.AI) when available, otherwise OpenAI-compatible cloud config.
/// </summary>
public sealed class MathBuddyService
{
	public const string SystemPrompt = """
		You are Math Buddy, a friendly math tutor embedded in a calculator app.
		Help learners understand arithmetic, order of operations, fractions, percentages,
		algebra basics, and multi-step word problems.

		Guidelines:
		- Be concise, encouraging, and clear.
		- Prefer step-by-step explanations with intermediate results.
		- Use get_calculator_state when you need the current display, in-progress expression, or recent calculations.
		- Use explain_current_calculation when the learner asks to explain the calculation currently on screen.
		- Use list_recent_history when the learner asks to check their last calculation or review previous calculations.
		- Do not invent calculator results; use tools or the provided expression context.
		- If no AI backend is available, the app will show configuration guidance instead of calling you.
		""";

	readonly IServiceProvider _services;
	readonly MathBuddyClientFactory _clientFactory;
	readonly CalculatorService _calculator;

	public MathBuddyService(
		IServiceProvider services,
		CalculatorService calculator,
		MathBuddyClientFactory clientFactory)
	{
		_services = services;
		_calculator = calculator;
		_clientFactory = clientFactory;
	}

	public bool IsAvailable => _clientFactory.IsAvailable;

	public string AvailabilityMessage => _clientFactory.AvailabilityMessage;

	public IChatClient? CreateToolClient()
	{
		var chatClient = _clientFactory.CreateChatClient();
		if (chatClient is null)
			return null;

		var builder = new ChatClientBuilder(chatClient);
		if (!_clientFactory.IsUsingAppleIntelligence)
		{
			builder.UseFunctionInvocation()
				.ConfigureOptions(options =>
				{
					options.Tools ??= [];
					foreach (var tool in MathBuddyToolContext.Default.Tools)
						options.Tools.Add(tool);
				});
		}

		return builder.Build(_services);
	}

	public async IAsyncEnumerable<string> StreamReplyAsync(
		IList<ChatMessage> history,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var client = CreateToolClient();
		if (client is null)
		{
			yield return AvailabilityMessage;
			yield break;
		}

		var messages = EnsureSystemPrompt(history);
		await foreach (var update in client.GetStreamingResponseAsync(
			messages,
			CreateChatOptions(),
			cancellationToken))
		{
			if (!string.IsNullOrEmpty(update.Text))
				yield return update.Text;
		}
	}

	public async Task<string> GetReplyAsync(
		IList<ChatMessage> history,
		CancellationToken cancellationToken = default)
	{
		var client = CreateToolClient();
		if (client is null)
			return AvailabilityMessage;

		var messages = EnsureSystemPrompt(history);
		var response = await client.GetResponseAsync(
			messages,
			CreateChatOptions(),
			cancellationToken);
		return string.IsNullOrWhiteSpace(response.Text)
			? "(no response)"
			: response.Text;
	}

	public Task<string> ExplainCurrentCalculationAsync(CancellationToken cancellationToken = default)
	{
		var expression = string.IsNullOrWhiteSpace(_calculator.Expression)
			? _calculator.Display
			: $"{_calculator.Expression} (display: {_calculator.Display})";

		var prompt = $"""
			Please tutor me through this calculator state.
			Current expression/display: {expression}
			Use tools if helpful, then explain the math step by step.
			""";

		return GetReplyAsync(
			[new ChatMessage(ChatRole.User, prompt)],
			cancellationToken);
	}

	ChatOptions? CreateChatOptions()
		=> _clientFactory.IsUsingAppleIntelligence
			? new ChatOptions { Tools = [.. MathBuddyToolContext.Default.Tools.OfType<AIFunction>()] }
			: null;

	static List<ChatMessage> EnsureSystemPrompt(IList<ChatMessage> history)
	{
		var messages = history.ToList();
		if (messages.Count == 0 || messages[0].Role != ChatRole.System)
			messages.Insert(0, new ChatMessage(ChatRole.System, SystemPrompt));
		return messages;
	}
}
