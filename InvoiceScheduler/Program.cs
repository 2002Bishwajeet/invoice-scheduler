using InvoiceScheduler.Models;
using InvoiceScheduler.Services;
using InvoiceScheduler.Utils;
using Newtonsoft.Json;


Console.WriteLine("Invoice Scheduler Started");

try
{
    var isRider = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RIDER_ENGINE"));
    if (isRider)
    {
        var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
        DotNetEnv.Env.Load(envPath);
        Console.WriteLine($"[env] Loaded .env from: {envPath} (Rider detected)");
    }
    else
    {
        DotNetEnv.Env.Load();
        Console.WriteLine("[env] Loaded .env from current working directory (non-Rider)");
    }
    var invoiceGenerator = new InvoiceGenerator();
    var pdfPath = await invoiceGenerator.GetInvoiceAsync();
    Console.WriteLine($"Invoice generated successfully at: {pdfPath}");
    //sending invoice
    var emailProvider = new EmailProvider();
    await emailProvider.SendInvoiceEmailAsync(pdfPath);

    // once email is sent, update the json
    var jsonContent = await File.ReadAllTextAsync("data.json");
    var data = JsonConvert.DeserializeObject<InvoiceData>(jsonContent)
              ?? throw new InvalidOperationException("Failed to deserialize invoice data");
    // Update invoice number to match the one just used
    data.InvoiceNumber = Utils.GenerateInvoiceNumber();
    var updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
    await File.WriteAllTextAsync("data.json", updatedJson);
    Console.WriteLine("data.json updated with new invoice number.");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to generate invoice: {ex.Message}");
    Environment.Exit(1);
}