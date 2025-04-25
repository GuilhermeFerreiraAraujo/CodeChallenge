using System.Globalization;

namespace Qplix.CodeChallenge.Utils;

public static class Conversions
{
    public static decimal StringToDecimal(this string number)
    {
        decimal localCultreResult;
        decimal.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out localCultreResult);

        return localCultreResult;
    }
}