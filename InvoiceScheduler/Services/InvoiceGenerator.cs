using InvoiceScheduler.Models;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using static InvoiceScheduler.Utils.Utils;

namespace InvoiceScheduler.Services;

public class InvoiceGenerator
{
    /// <summary>
    /// Generates an invoice PDF using data from a local JSON file,
    /// fills the invoicer web app using Puppeteer, and saves the PDF to disk.
    /// </summary>
    /// <returns>The file path of the generated invoice PDF.</returns>
    /// <exception cref="Exception">Thrown when invoice generation fails.</exception>
    public async Task<string> GetInvoiceAsync()
    {
        try
        {
            // Compute the invoice file path for the current month and year
            var invoicesDir = Path.Combine(AppContext.BaseDirectory, "..", "invoices");
            invoicesDir = Path.GetFullPath(invoicesDir);
            var monthYear = GetPreviousMonthDate(DateTime.Now);
            var fileName = $"invoice-{monthYear}.pdf";
            var filePath = Path.Combine(invoicesDir, fileName);

            // Check if the invoice already exists
            if (File.Exists(filePath))
            {
                Console.WriteLine($"Invoice already exists: {filePath}");
                return filePath;
            }

            Console.WriteLine("Loading invoice data...");


            // Use data.json from the top-level repository directory
            string dataJsonPath;
            var isRider = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RIDER_ENGINE"));
            if (isRider)
            {
                // Rider runs from project root, so use bin-relative path
                dataJsonPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "data.json"));
            }
            else
            {
                // CLI and most IDEs: use current working directory (repo root)
                dataJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data.json");
            }
            var jsonContent = await File.ReadAllTextAsync(dataJsonPath);
            var data = JsonConvert.DeserializeObject<InvoiceData>(jsonContent)
                      ?? throw new InvalidOperationException("Failed to deserialize invoice data");

            // Set the invoice number (you can modify this logic as needed)
            data.InvoiceNumber = GenerateInvoiceNumber();

            var stringifyData = JsonConvert.SerializeObject(data);

            Console.WriteLine("Launching browser...");

            // Download chromium if not already downloaded
            await new BrowserFetcher().DownloadAsync();

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" }
            });

            var page = await browser.NewPageAsync();

            Console.WriteLine("Navigating to invoicer website...");

            // Set a longer timeout and wait for the page to load completely
            await page.GoToAsync("https://invoicer.cloudx.run", new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                Timeout = 60000
            });

            Console.WriteLine("Setting localStorage data...");

            // Set localStorage data
            await page.EvaluateFunctionAsync(@"(jsonData) => {
                localStorage.setItem('previewOnly', 'true');
                localStorage.setItem('invoiceFormData', jsonData);
            }", stringifyData);

            Console.WriteLine("Reloading page...");
            await page.ReloadAsync(new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            Console.WriteLine("Generating PDF...");
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                PreferCSSPageSize = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0",
                    Right = "0",
                    Bottom = "0",
                    Left = "0"
                }
            });

            // Create a new directory if it doesn't exist
            if (!Directory.Exists(invoicesDir))
            {
                Directory.CreateDirectory(invoicesDir);
            }

            Console.WriteLine("Saving PDF to file...");


            await File.WriteAllBytesAsync(filePath, pdfBytes);

            Console.WriteLine("Closing browser...");
            await browser.CloseAsync();

            Console.WriteLine("Invoice PDF generated successfully!");
            // Write updated data.json back to the top-level directory if needed
            var updatedJson = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(dataJsonPath, updatedJson);
            return filePath;
        }
        catch (Exception error)
        {
            Console.WriteLine($"Error generating invoice: {error}");
            throw;
        }
    }


}
