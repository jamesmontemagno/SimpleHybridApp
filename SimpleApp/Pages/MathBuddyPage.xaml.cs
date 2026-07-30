using System.Collections.Specialized;
using SimpleApp.ViewModels;

namespace SimpleApp.Pages;

public partial class MathBuddyPage : ContentPage
{
	MathBuddyViewModel? _vm;

	public MathBuddyPage()
	{
		InitializeComponent();

		var services = IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("MAUI services are not available.");
		_vm = services.GetRequiredService<MathBuddyViewModel>();
		BindingContext = _vm;
		_vm.Messages.CollectionChanged += OnMessagesChanged;
	}

	void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action != NotifyCollectionChangedAction.Add || _vm is null || _vm.Messages.Count == 0)
			return;

		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
		{
			try
			{
				MessagesView.ScrollTo(_vm.Messages[^1], position: ScrollToPosition.End, animate: true);
			}
			catch
			{
				// Layout may not be ready yet.
			}
		});
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_vm?.RefreshAvailability();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		if (_vm is not null)
			_vm.Messages.CollectionChanged -= OnMessagesChanged;
	}
}
