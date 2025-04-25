using Qplix.CodeChallenge.Utils;
namespace Qplix.CodeChallenge.Tests;

[TestFixture]
public class FormatTests
{
    [Test]
    public void FormatTests_DecimalToMoney_True()
    {
        decimal decimalNumber = 10;
        var strNumber = decimalNumber.DecimalToMoney();
        
        Assert.That(strNumber, Is.EqualTo("€10,00"));
    }
}