using System.Text;

namespace JAMFR.Logic.Implementations;

public static class TagTextRepair
{
    // Windows-1251 = Cyrillic codepage commonly misread as Latin-1/CP1252
    private static readonly Encoding Cp1251 = Encoding.GetEncoding(1251);
    private static readonly Encoding Latin1 = Encoding.GetEncoding("ISO-8859-1");

    /// <summary>
    /// Detects and repairs strings that were Cyrillic (CP1251) bytes
    /// but got decoded as Latin-1, producing mojibake like "Êèíî".
    /// </summary>
    public static string FixMojibake(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        if (!LooksLikeMojibake(input))
            return input;

        try
        {
            // Reverse the wrong decode: string -> original bytes (as Latin-1),
            // then decode those same bytes correctly as CP1251.
            var originalBytes = Latin1.GetBytes(input);
            var repaired = Cp1251.GetString(originalBytes);

            // Sanity check: repaired text should now contain actual Cyrillic
            // letters, not more garbage. If not, bail and keep the original.
            return ContainsCyrillic(repaired) ? repaired : input;
        }
        catch
        {
            // If anything goes wrong (invalid byte sequence etc.), don't
            // corrupt the tag further — just return the original text.
            return input;
        }
    }

    private static bool LooksLikeMojibake(string s)
    {
        // Heuristic: mojibake from CP1251-as-Latin1 is dominated by
        // Latin-1 Supplement characters (U+00C0–U+00FF range) and has
        // no Cyrillic characters yet.
        if (ContainsCyrillic(s))
            return false;

        int suspicious = 0;
        foreach (var c in s)
        {
            if (c >= 0x00C0 && c <= 0x00FF)
                suspicious++;
        }

        // If a meaningful chunk of the string is in that range, it's
        // very likely misdecoded Cyrillic rather than genuine Latin text.
        return s.Length > 0 && (double)suspicious / s.Length > 0.3;
    }

    private static bool ContainsCyrillic(string s)
    {
        foreach (var c in s)
        {
            if (c >= 0x0400 && c <= 0x04FF)
                return true;
        }
        return false;
    }
}