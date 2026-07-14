using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using SimpleApp.Services;

namespace SimpleApp.ViewModels;

public sealed partial class MathBuddyViewModel : ObservableObject
{
	readonly MathBuddyService _mathBuddy;
	readonly List<ChatMessage> _history = [];
	CancellationTokenSource? _cts;

	public MathBuddyViewModel(MathBuddyService mathBuddy)
	{
		_mathBuddy = mathBuddy;
		StatusText = mathBuddy.AvailabilityMessage;
		Messages.Add(new MathBuddyMessage("assistant", mathBuddy.IsAvailable
			? "Hi! I'm Math Buddy. Ask me about a problem, or open Calculator and tap Explain for the current expression."
			: mathBuddy.AvailabilityMessage));
	}

	public ObservableCollection<MathBuddyMessage> Messages { get; } = [];

	public IReadOnlyList<string> Suggestions { get; } =
	[
		"Explain order of operations",
		"How do I multiply fractions?",
		"Why is 20 + 5 = 25?",
		"Help me check my last calculation"
	];

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsNotBusy))]
	[NotifyCanExecuteChangedFor(nameof(SendCommand))]
	[NotifyCanExecuteChangedFor(nameof(ExplainCurrentCommand))]
	public partial bool IsBusy { get; set; }

	public bool IsNotBusy => !IsBusy;

	[ObservableProperty]
	public partial string StatusText { get; set; } = string.Empty;

	public bool CanSend => !IsBusy && !string.IsNullOrWhiteSpace(InputText);

	public void RefreshAvailability()
	{
		if (!IsBusy)
			StatusText = _mathBuddy.AvailabilityMessage;
	}

	partial void OnInputTextChanged(string value)
		=> SendCommand.NotifyCanExecuteChanged();

	[RelayCommand(CanExecute = nameof(CanSend))]
	async Task SendAsync()
	{
		var text = InputText.Trim();
		if (string.IsNullOrWhiteSpace(text))
			return;

		InputText = string.Empty;
		await AskAsync(text);
	}

	[RelayCommand]
	async Task RunSuggestionAsync(string? prompt)
	{
		if (string.IsNullOrWhiteSpace(prompt) || IsBusy)
			return;

		await AskAsync(prompt);
	}

	[RelayCommand(CanExecute = nameof(IsNotBusy))]
	async Task ExplainCurrentAsync()
	{
		if (IsBusy)
			return;

		await AskAsync("Explain my current calculator expression step by step.");
	}

	[RelayCommand]
	void ClearChat()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
		_history.Clear();
		Messages.Clear();
		Messages.Add(new MathBuddyMessage("assistant", "Chat cleared. What would you like to learn?"));
		StatusText = _mathBuddy.AvailabilityMessage;
		IsBusy = false;
	}

	async Task AskAsync(string userText)
	{
		IsBusy = true;
		StatusText = "Thinking...";
		Messages.Add(new MathBuddyMessage("user", userText));
		_history.Add(new ChatMessage(ChatRole.User, userText));

		var assistant = new MathBuddyMessage("assistant", string.Empty);
		Messages.Add(assistant);

		_cts?.Cancel();
		_cts?.Dispose();
		_cts = new CancellationTokenSource();

		try
		{
			var sb = new System.Text.StringBuilder();
			await foreach (var chunk in _mathBuddy.StreamReplyAsync(_history, _cts.Token))
			{
				sb.Append(chunk);
				assistant.Text = sb.ToString();
			}

			if (string.IsNullOrWhiteSpace(assistant.Text))
				assistant.Text = "(no response)";

			_history.Add(new ChatMessage(ChatRole.Assistant, assistant.Text));
			StatusText = _mathBuddy.IsAvailable ? "Ready" : _mathBuddy.AvailabilityMessage;
		}
		catch (OperationCanceledException)
		{
			assistant.Text = string.IsNullOrWhiteSpace(assistant.Text) ? "(cancelled)" : assistant.Text;
			StatusText = "Cancelled";
		}
		catch (Exception ex)
		{
			assistant.Text = $"Error: {ex.Message}";
			StatusText = "Error";
		}
		finally
		{
			IsBusy = false;
			_cts?.Dispose();
			_cts = null;
		}
	}
}

public sealed partial class MathBuddyMessage : ObservableObject
{
	public MathBuddyMessage(string role, string text)
	{
		Role = role;
		Text = text;
	}

	public string Role { get; }

	public bool IsUser => Role.Equals("user", StringComparison.OrdinalIgnoreCase);

	[ObservableProperty]
	public partial string Text { get; set; } = string.Empty;
}
