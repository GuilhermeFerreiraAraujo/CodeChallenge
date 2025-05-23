using Models;
using Models.Responses;
using Qplix.CodeChallenge.Enums;
using Qplix.CodeChallenge.Interfaces;

namespace Qplix.CodeChallenge.Services;

public class PortfolioServices : IPortfolioServices
{

    private readonly List<Investment> _investments;
    private readonly List<Transaction> _transactions;
    private readonly List<Quote> _quotes;
    private List<Transaction> _investmentsPercentages;
    private List<Transaction> _shares;
    private List<Investment> _foundsInvestments;
    private List<Transaction> _realStateTransactions;

    public PortfolioServices(List<Transaction> transactions, List<Quote> quotes, List<Investment> investments,
        List<Transaction> investmentsPercentages, List<Transaction> shares,
        List<Investment> foundsInvestments,
        List<Transaction> realStateTransactions)
    {
        _investments = investments;
        _transactions = transactions;
        _quotes = quotes;
        _investmentsPercentages = investmentsPercentages;
        _shares = shares;
        _foundsInvestments = foundsInvestments;
        _realStateTransactions = realStateTransactions;
    }
    
    public decimal GetTotalInvestorStocksValue(DateTime date, string investorId)
    {
        var stocks = _investments.Where(i => i.InvestmentType == InvestmentTypes.Stock && i.InvestorId == investorId).ToList();

        var summedTransactions = _shares
            .Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new { InvestmentId = x.Key, Value = x.Sum(i => i.Value) });

        var quotes = GetStockQuotes(date);
        
        var stockTransactions = from stock in stocks

            join share in summedTransactions on stock.InvestmentId equals share.InvestmentId
            join quote in quotes on stock.Isin equals quote.Isin

            select new
            { 
                stock.Isin,
                stock.InvestmentId,
                Amount = share.Value,
                Price = quote.Value,
                Total = CalculateStockValue(quote.Value, share.Value)
            };

        var sum = stockTransactions.Sum(x => x.Total);
        
        return sum;
    }
    
    
    public decimal GetTotalInvestorRealStateValue(DateTime date, string investorId)
    {
        var realStateInvestments = _investments.Where(x => x.InvestmentType == InvestmentTypes.RealEstate && x.InvestorId== investorId).ToList();
        
        var summedTransactions = _realStateTransactions.Where(x => x.Date <= date)
            .Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new { InvestmentId = x.Key, Value = x.Sum(i => i.Value) }).ToList();
        
        var query = from stock in realStateInvestments

            join share in summedTransactions on stock.InvestmentId equals share.InvestmentId

            select new
            { 
                stock.InvestmentId,
                share.Value,
            };
        
        return query.Sum(x => x.Value);
    }
    
    public decimal GetTotalInvestorFoundsValue(DateTime date, string investorId)
    {
        var percentages = _investmentsPercentages.Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new {InvestmentId = x.Key, Value = x.Sum(y => y.Value) });

        var investments = _investments.Where(x => x.InvestmentType == InvestmentTypes.Fonds && x.InvestorId == investorId).ToList();
        
        var query = from percentage in percentages
            join investment in investments
                on percentage.InvestmentId equals investment.InvestmentId
            select new
            {
                investment.FoundsInvestor,
                investment.InvestmentId,
                percentage.Value,
            };
        
        var foundsPercentage = query.GroupBy(x => x.FoundsInvestor)
            .Select(x => new { FoundsInvestor = x.Key, Value = x.Sum(y => y.Value) });
        
        var foundIds = investments.Select(x => x.FoundsInvestor).Distinct();
        
        var foundValues = GetFoundsValuesByIds(foundIds, date);

        var groupDateQuery = from percentage in foundsPercentage
            join value in foundValues
                on percentage.FoundsInvestor equals value.FoundId
            select new
            {
                value.FoundId,
                Value = CalculateFoundsTotalInvested(value.Value, percentage.Value),
            };
        
        var response = groupDateQuery.Sum(x => x.Value);
        
        return response;
    }

    public decimal CalculateFoundsTotalInvested(decimal foundsValue, decimal percentage)
    {
        return foundsValue * percentage;
    }
    
    public IEnumerable<FoundValue> GetFoundsValuesByIds(IEnumerable<string> ids, DateTime date)
    {
        var summedTransactions = _shares
            .Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new { InvestmentId = x.Key, Value = x.Sum(i => i.Value) });

        var investments = _investments.Where(x => ids.Contains(x.InvestorId));

        var foundsGrouped = investments.GroupBy(x => new { FoundId = x.InvestorId, InvestmentId = x.InvestmentId });
        
        var groupDateQuery = from transaction in summedTransactions
            join investment in investments
                on transaction.InvestmentId equals investment.InvestmentId
            select new
            {
                transaction.InvestmentId,
                transaction.Value
            };
        
        var query = from founds in foundsGrouped
            join Transaction in groupDateQuery
                on founds.Key.InvestmentId equals Transaction.InvestmentId
            select new
            {
                Transaction.InvestmentId,
                founds.Key.FoundId,
                Transaction.Value,
            };

        var foundsValues = query.GroupBy(x => x.FoundId)
            .Select(x => new FoundValue() { FoundId = x.Key, Value = x.Sum(i => i.Value) });

        return foundsValues;
    }
    
    public decimal CalculateStockValue(decimal price, decimal amount)
    {
        return price * amount;
    }

    public IEnumerable<StockQuote> GetStockQuotes(DateTime date)
    {
        var result = _quotes
            .Where(x => x.Date <= date)
            .GroupBy(x => x.Isin, (key,g) => g.OrderByDescending(e => e.PricePerShare).First())
            .Select(x => new StockQuote {Isin = x.Isin, Value = x.PricePerShare});

        return result;
    }
    
    public PortfolioTotalResponse GetPortfolioValue(DateTime date, string investorId)
    {
        var totalInStocks = GetTotalInvestorStocksValue(date, investorId);

        var totalInFounds = GetTotalInvestorFoundsValue(date, investorId);

        var realStateTotal = GetTotalInvestorRealStateValue(date, investorId);

        return new PortfolioTotalResponse
        {
            TotalStocks = totalInStocks,
            TotalRealState = realStateTotal,
            TotalFounds = totalInFounds,
        };
    }

    public bool IsClient(string investorId)
    {
        return _investments.Any(x => x.InvestorId == investorId);
    }
}