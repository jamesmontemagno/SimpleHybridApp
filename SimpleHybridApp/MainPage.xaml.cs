namespace SimpleHybridApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            hybridWebView.SetInvokeJavaScriptTarget(this);
        }

		private async void OnHybridWebViewRawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
		{
            if (e.Message != "Celebrate")
                return;

            await Dispatcher.DispatchAsync(async () =>
            {

                var celebrate = await DisplayAlert("Celebrate", "Do you want to celebrate?", "Yes", "No");

                if (celebrate)
                    hybridWebView.SendRawMessage("Celebrate");
            });
		}
	}

}
