using System.Globalization;

public static class LocalizedNumberExtensions
{
    private static string _cachedLang;
    private static string[] _cachedSuffixes;

    private static string[] GetSuffixes()
    {
        string lang = LocalizationManager.CurrentLanguage;
        if (_cachedSuffixes == null || _cachedLang != lang)
        {
            _cachedLang = lang;
            string raw = LocalizationManager.Get("number.suffixes");
            _cachedSuffixes = raw.StartsWith("[")
                ? new[] { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" }
                : raw.Split(',');
        }
        return _cachedSuffixes;
    }

    public static string ToLocalizedCompact(this double num)
    {
        if (double.IsNaN(num)) return "0";
        if (double.IsPositiveInfinity(num)) return LocalizationManager.Get("number.max");
        if (num < 1000) return num.ToString("0", CultureInfo.InvariantCulture);

        string[] suffixes = GetSuffixes();
        int suffixIndex = 0;
        double originalNum = num;

        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        if (num >= 1000 && suffixIndex == suffixes.Length - 1)
            return originalNum.ToString("0.00e0", CultureInfo.InvariantCulture);

        return num.ToString("0.0", CultureInfo.InvariantCulture) + suffixes[suffixIndex];
    }
}