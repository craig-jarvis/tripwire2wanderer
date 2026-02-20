using System.Text.RegularExpressions;

namespace Tripwire2Wanderer;

public partial class AppRegex
{
    public const string EveIdPattern = @"^[A-Z]{3}-\d{3}$";
    public const string TripWireSignatureIdPattern = "^[A-Za-z]{3}\\d{3}$";

    [GeneratedRegex(EveIdPattern)]
    public static partial Regex EveIdRegex();

    [GeneratedRegex(TripWireSignatureIdPattern)]
    public static partial Regex TripWireSignatureIdRegex();
}