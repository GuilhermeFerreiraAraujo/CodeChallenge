using Models;
using Models.Responses;
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

 
    public decimal GetTotalRealStateValue(DateTime date, string investorId)
    {
        return 0;
    }

    public decimal GetTotalStocksValue(DateTime date, string investorId)
    {

        return 0;
    }


    public decimal GetTotalInvestorStocksValue(DateTime date, string investorId)
    {
        var stocks = _investments.Where(i => i.InvestmentType == InvestmentTypes.Stock && i.InvestorId == investorId).ToList();

        var summedTransactions = _shares
            .Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new { InvestmentId = x.Key, Value = x.Sum(i => i.Value) }).ToList();

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

        var queryResult = stockTransactions.ToList();
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
                Value = share.Value,
            };
        
        return query.Sum(x => x.Value);
    }
    
    public decimal GetTotalInvestorFoundsValue(DateTime date, string investorId)
    {
        var percentages = _investmentsPercentages.Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new {InvestmentId = x.Key, Value = x.Sum(y => y.Value) })
            .ToList();

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
            .Select(x => new { FoundsInvestor = x.Key, Value = x.Sum(y => y.Value) }).ToList();
        
        var foundIds = investments.Select(x => x.FoundsInvestor).Distinct().ToList();
        
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
    
    public List<FoundValue> GetFoundsValuesByIds(List<string> ids, DateTime date)
    {
        var summedTransactions = _shares
            .Where(x => x.Date <= date)
            .GroupBy(x => x.InvestmentId)
            .Select(x => new { InvestmentId = x.Key, Value = x.Sum(i => i.Value) }).ToList();

        var investments = _investments.Where(x => ids.Contains(x.InvestorId)).ToList();

        var foundsGrouped = investments.GroupBy(x => new { FoundId = x.InvestorId, InvestmentId = x.InvestmentId });
        
        var groupDateQuery = from transaction in summedTransactions
            join investment in investments
                on transaction.InvestmentId equals investment.InvestmentId
            select new
            {
                transaction.InvestmentId,
                transaction.Value
            };

        var transactions = groupDateQuery.ToList();
        
        
        var query = from founds in foundsGrouped
            join Transaction in transactions
                on founds.Key.InvestmentId equals Transaction.InvestmentId
            select new
            {
                Transaction.InvestmentId,
                founds.Key.FoundId,
                Transaction.Value,
            };

        var foundsValues = query.GroupBy(x => x.FoundId)
            .Select(x => new FoundValue() { FoundId = x.Key, Value = x.Sum(i => i.Value) }).ToList();

        return foundsValues;
    }
    
    public decimal CalculateStockValue(decimal price, decimal amount)
    {
        return price * amount;
    }

    public List<StockQuote> GetStockQuotes(DateTime date)
    {
        var result = _quotes
            .Where(x => x.Date <= date)
            .GroupBy(x => x.Isin, (key,g) => g.OrderByDescending(e => e.PricePerShare).First())
            .Select(x => new StockQuote {Isin = x.Isin, Value = x.PricePerShare}).ToList();

        return result;
    }
    
    public PortfolioTotalResponse GetPortfolioValue(DateTime date, string investorId)
    {
        var totalInStocks = GetTotalStocksValue(date, investorId);

        var totalInFounds = GetTotalInvestorFoundsValue(date, investorId);

        var realStateTotal = GetTotalInvestorRealStateValue(date, investorId)

        return new PortfolioTotalResponse
        {
            TotalStocks = totalInStocks,
            TotalRealState = realStateTotal,
            TotalFounds = totalInFounds,
        };
    }

    public decimal GetFoundsPercentage(decimal percentage, decimal totalFound)
    {
        return percentage * totalFound;
    }
    
    public decimal GetFoundsInvestedByInvestmentId(List<Transaction> transactions,string InvestmentId, DateTime date)
    {
        var value = transactions.Where(x => x.InvestmentId == InvestmentId && x.Date <= date).Sum(x => x.Value);
        return value;
    }
    
    public decimal GetFoundsValue(string fondId)
    {
        var founds = _investments.Where(x => x.FoundsInvestor == fondId)
            .Select(x => x.InvestmentId).ToList();

        var transasctions = GetFoundsTransactions(founds);
        
        var total = transasctions.Sum(x => x.Value);

        return total;

    }

    public List<Transaction> GetFoundsTransactions(List<string> transactionIds)
    {
        var transactions = _transactions.Where(x => transactionIds.Contains(x.InvestmentId)).ToList();
        return transactions;
    }
    
    public List<Transaction> GetRealStateTransactions(List<Transaction> transactions, List<string> investmentIds)
    {
        return transactions.Where(x => investmentIds.Contains(x.InvestmentId)).ToList();
    }
    

   


    public List<Investment> GetInvestments(string InvestorId)
    {
        return _investments.Where(x=>x.InvestorId == InvestorId).ToList();
    }

    public List<Investment> GetStocksFromInvestments(List<Investment> investments)
    {
        return investments.Where(x => x.InvestmentType == InvestmentTypes.Stock).ToList();
    }
    
    public List<Investment> GetRealStateFromInvestments(List<Investment> investments)
    {
        return investments.Where(x => x.InvestmentType == InvestmentTypes.RealEstate).ToList();
    }
    
    public List<Investment> GetFondsFromInvestments(List<Investment> investments)
    {
        return investments.Where(x => x.InvestmentType == InvestmentTypes.Fonds).ToList();
    }
    
    
    
    public decimal GetStockAmount(List<Transaction> transactions, string investmentId, DateTime date)
    {
        var value = transactions.Where(x=>x.InvestmentId == investmentId && x.Date <= date)
            .GroupBy(t => t.InvestmentId)
            .Select(g => g.Sum(t => t.Value)).FirstOrDefault();
        return value;
    }


    public decimal GetStockPrice(string isin, DateTime date)
    {
        var price = _quotes.Where(x => x.Isin == isin && x.Date <= date)
            .OrderByDescending(x => x.Date)
            .First().PricePerShare;

        return price;
    }
    
    
    public List<Quote> GetStockPrices(string isin)
    {
        return _quotes.Where(x=> x.Isin == isin).ToList();  
    }
    
    
    public List<Transaction> GetStockTransactions(string investmentId)
    {
        
        var list = _transactions.Where(t => t.InvestmentId == investmentId).ToList();
        return list;
        
    }

    
    
    public List<Transaction> GetTransactions(DateTime date, string InvestorId)
    {
        
        var transactions = _transactions.Where(x=> x.InvestmentId == "Investment12537");
        
        
        return transactions.ToList();
        
    }
    
    
    public List<Investment> GetStocks()
    {
        return _investments.Where(x => x.InvestmentType == InvestmentTypes.Stock).ToList();
    }
    public List<string> GetInvestmentTypes()
    {
        var investmentTypes = _investments.GroupBy(i => i.InvestmentType).Select(i => i.Key.ToString()).ToList();
        return investmentTypes;
    }

    public List<Investment> GetInvestorInvestments(string InvestorId)
    {
        return _investments.Where(x => x.InvestorId == InvestorId).ToList();
    }


}