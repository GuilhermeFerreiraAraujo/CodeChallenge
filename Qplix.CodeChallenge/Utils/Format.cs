using System.Globalization;

namespace Qplix.CodeChallenge.Utils;

public static class Format
{
    public static string DecimalToMoney(this decimal number)
    {
        return number.ToString("C");
    }
}