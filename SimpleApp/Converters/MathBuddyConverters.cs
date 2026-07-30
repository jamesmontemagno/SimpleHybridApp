using System.Globalization;

namespace SimpleApp.Converters;

public sealed class BoolToLayoutOptionsConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? LayoutOptions.End : LayoutOptions.Start;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

public sealed class BoolToBubbleBrushConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var isUser = value is true;
		if (isUser)
		{
			if (Application.Current?.Resources.TryGetValue("Primary", out var primary) == true && primary is Color primaryColor)
				return new SolidColorBrush(primaryColor);
			return new SolidColorBrush(Colors.MediumPurple);
		}

		var surface = Application.Current?.RequestedTheme == AppTheme.Dark
			? Color.FromArgb("#2B2B2B")
			: Color.FromArgb("#F0F0F0");
		return new SolidColorBrush(surface);
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}

public sealed class BoolToBubbleTextConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var isUser = value is true;
		if (isUser)
			return Colors.White;

		return Application.Current?.RequestedTheme == AppTheme.Dark
			? Colors.White
			: Colors.Black;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
