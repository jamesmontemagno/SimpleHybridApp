using System.Text.Json.Serialization;

namespace SimpleApp.Services;

public sealed class MathBuddySettings
{
	public string ApiKey { get; set; } = string.Empty;

	public string Endpoint { get; set; } = string.Empty;

	public string Model { get; set; } = "gpt-4o-mini";

	[JsonIgnore]
	public string Source { get; set; } = string.Empty;

	[JsonIgnore]
	public bool HasCloudConfiguration => !string.IsNullOrWhiteSpace(ApiKey);

	[JsonIgnore]
	public string ModelOrDefault => string.IsNullOrWhiteSpace(Model)
		? "gpt-4o-mini"
		: Model.Trim();
}