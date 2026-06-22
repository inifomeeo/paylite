namespace api.Options;

public class PaymentOptions
{
    public const string SectionName = "Payments";

    public string[] SupportedCurrencies { get; set; } = [];
}
