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
        var monthName = date.ToString("MMMM");
        var year = date.Year;
        return $"{monthName}-{year}";
    }

    /// <summary>
    /// Returns the first day of the previous month based on the given date (or now if not provided).
    /// </summary>
    /// <param name="reference">The reference date. If null, uses DateTime.Now.</param>
    /// <returns>DateTime for the first day of the previous month.</returns>
    public static string GetPreviousMonthDate(DateTime? reference = null)
    {
        var date = reference ?? DateTime.Now;
        var previousMonthDate = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
        var monthName = previousMonthDate.ToString("MMMM");
        var year = previousMonthDate.Year;
        return $"{monthName}-{year}";
    }

    public const string Defaultpayloadkey = "dflt_key";
    public const int MaxHeaderContentBytes = 7000;
}