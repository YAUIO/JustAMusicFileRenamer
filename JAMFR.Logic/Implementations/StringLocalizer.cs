using UnidecodeSharpFork;

namespace JAMFR.Logic.Implementations;

public class StringLocalizer : IStringLocalizer
{
    private readonly char[] _chars = Path.GetInvalidFileNameChars();
    
    public string Localize(string input) 
        => string.Join("",
            input
            .Unidecode()
            .Trim()
            .Replace("\0", string.Empty)
            .Replace("---", "-")
            .Where(c => !_chars.Contains(c)));
}