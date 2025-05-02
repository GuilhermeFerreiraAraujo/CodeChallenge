using Qplix.CodeChallenge.Utils;
namespace Qplix.CodeChallenge.Tests;

[TestFixture]
public class ConversionsTests
{
    [Test]
    public void ConversionTests_StringToDecimal_True()
    {
        var strNumber = "10".StringToDecimal();
        
        Assert.That(strNumber, Is.EqualTo(10));
    }
    
}