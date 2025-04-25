// See https://aka.ms/new-console-template for more information

using Models;
using Qplix.CodeChallenge;
using Qplix.CodeChallenge.Constants;
using Qplix.CodeChallenge.Enums;
using Qplix.CodeChallenge.Services;
using Qplix.CodeChallenge.Utils;


try
{
    
    #region PROJECT SET UP, reading files, loading datasets...

    var investmentFileReader = new FileReader<Investment>(Config.FilePath,"Investments.csv");
    var transactionFileReader = new FileReader<Transaction>(Config.FilePath,"Transactions.csv");
    var quoteFileReader = new FileReader<Quote>(Config.FilePath,"Quotes.csv"); 

    var investments = investmentFileReader.ReadFile();
    var transactions = transactionFileReader.ReadFile();
    var quotes = quoteFileReader.ReadFile().ToList();
    
    var percentages = transactions.Where(x => x.Type == TransactionTypes.Percentage).ToList();
    var shares = transactions.Where(x => x.Type == TransactionTypes.Shares).ToList();
    var realStateTransactions = transactions.Where(x => x.Type == TransactionTypes.Building || x.Type == TransactionTypes.Estate).ToList();

    var foundsInvestments = investments.Where(x => x.InvestorId.Contains("Fonds")).ToList();

    var portfolioServices = new PortfolioServices(transactions, quotes, investments, percentages, shares, foundsInvestments, realStateTransactions);

    #endregion

    
    Console.WriteLine(Labels.FIRST_INSTRUCTION);

    var line = Console.ReadLine();
    
    while (!string.IsNullOrWhiteSpace(line))
    {
        var input = line.Split(Config.CsvDelimiter);
    
        DateTime date;
        if(DateTime.TryParse(input[0], out date))
        {
            var investorId = input[1];

            if (portfolioServices.IsClient(investorId))
            {
                var result = portfolioServices.GetPortfolioValue(date, investorId);
    
                Console.WriteLine($"Total founds {result.TotalFounds.DecimalToMoney()}");
                Console.WriteLine($"Total stocks {result.TotalStocks.DecimalToMoney()}");
                Console.WriteLine($"Total real state {result.TotalRealState.DecimalToMoney()}");
                Console.WriteLine($"Total {(result.TotalFounds + result.TotalStocks + result.TotalRealState).DecimalToMoney()}");
            }
            else
            {
                Console.WriteLine(Labels.NOT_VALID_CLIENT);
            }
        }
        else
        {
            Console.WriteLine(Labels.INVALID_DATE);
        }
    
        line = Console.ReadLine();
    }
    
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.ReadLine();
    return;
}





