namespace api.Options;

public class SecurityOptions
{
    public const string SectionName = "Security";

    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
