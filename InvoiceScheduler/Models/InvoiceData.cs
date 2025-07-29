using Newtonsoft.Json;

namespace InvoiceScheduler.Models;

public class InvoiceData
{
    [JsonProperty("yourName")]
    public string YourName { get; set; } = string.Empty;

    [JsonProperty("yourEmail")]
    public string YourEmail { get; set; } = string.Empty;

    [JsonProperty("yourWebsite")]
    public string YourWebsite { get; set; } = string.Empty;

    [JsonProperty("taxId")]
    public string TaxId { get; set; } = string.Empty;

    [JsonProperty("yourAddress")]
    public string YourAddress { get; set; } = string.Empty;

    [JsonProperty("clientName")]
    public string ClientName { get; set; } = string.Empty;

    [JsonProperty("clientAddress")]
    public string ClientAddress { get; set; } = string.Empty;

    [JsonProperty("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonProperty("services")]
    public List<ServiceItem> Services { get; set; } = new List<ServiceItem>();

    [JsonProperty("savedAt")]
    public string SavedAt { get; set; } = string.Empty;
}

public class ServiceItem
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("quantity")]
    public string Quantity { get; set; } = string.Empty;

    [JsonProperty("rate")]
    public string Rate { get; set; } = string.Empty;
}
