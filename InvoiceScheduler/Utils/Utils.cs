namespace InvoiceScheduler.Utils;

public static class Utils
{
    /// <summary>
    /// Generates an invoice number in the format month/currentYear-nextYear (e.g., "7/25-26").
    /// </summary>
    /// <returns>An invoice number string.</returns>
    public static string GenerateInvoiceNumber()
    {
        var now = DateTime.Now;
        var month = now.Month; // 1-12
        var currentYear = now.Year;
        var currentYearShort = currentYear.ToString().Substring(2); // Last 2 digits
        var nextYearShort = (currentYear + 1).ToString().Substring(2); // Last 2 digits of next year
        return $"{month}/{currentYearShort}-{nextYearShort}";
    }

    /// <summary>
    /// Gets the month and year string in MM-yyyy format (e.g., "07-2025").
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>A formatted month-year string.</returns>
    public static string GetMonthYearString(DateTime date)
    {
        var month = date.Month.ToString().PadLeft(2, '0');
        var year = date.Year;
        return $"{month}-{year}";
    }
    
        public const string Defaultpayloadkey = "dflt_key";
        public const int MaxHeaderContentBytes = 7000;
}