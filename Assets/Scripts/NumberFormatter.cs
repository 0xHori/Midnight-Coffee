using System.Globalization;

public static class NumberFormatter
{
    private static readonly string[] Suffixes =
    {
        "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc"
    };

    public static string ToCompactString(this double num)
    {
        if (double.IsNaN(num)) return "0";
        if (double.IsPositiveInfinity(num)) return "MAX";
        if (num < 1000) return num.ToString("0", CultureInfo.InvariantCulture);

        int suffixIndex = 0;
        double originalNum = num;

        while (num >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        if (num >= 1000 && suffixIndex == Suffixes.Length - 1)
        {
            return originalNum.ToString("0.00e0", CultureInfo.InvariantCulture);
        }

        return num.ToString("0.0", CultureInfo.InvariantCulture) + Suffixes[suffixIndex];
    }
}