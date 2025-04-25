using Models;
using Qplix.CodeChallenge.Enums;
using Qplix.CodeChallenge.Services;
using Qplix.CodeChallenge.Utils;
namespace Qplix.CodeChallenge.Tests;

[TestFixture]
public class PortfolioServicesTests
{

    private PortfolioServices _portfolioServices;
    
    [SetUp]
    public void SetUp()
    {

        
        var investments = new List<Investment>();
        var transactions = new List<Transaction>();
        var quotes = new List<Quote>();
    
        var percentages = transactions.Where(x => x.Type == TransactionTypes.Percentage).ToList();
        var shares = transactions.Where(x => x.Type == TransactionTypes.Shares).ToList();
        var realStateTransactions = transactions.Where(x => x.Type == TransactionTypes.Building || x.Type == TransactionTypes.Estate).ToList();

        var foundsInvestments = investments.Where(x => x.InvestorId.Contains("Fonds")).ToList();

        _portfolioServices = new PortfolioServices(transactions, quotes, investments, percentages, shares, foundsInvestments, realStateTransactions);

        
    }
    
    [Test]
    public void PortfolioServicesTests_CalculateStockValue_True()
    {
        var calculation = _portfolioServices.CalculateStockValue(10, 100);
        
        Assert.That(calculation, Is.EqualTo(1000));
    }
    
    [Test]
    public void PortfolioServicesTests_CalculateFoundsValue_True()
    {
        var calculation = _portfolioServices.CalculateFoundsTotalInvested(10, 100);
        
        Assert.That(calculation, Is.EqualTo(1000));
    }
    
}