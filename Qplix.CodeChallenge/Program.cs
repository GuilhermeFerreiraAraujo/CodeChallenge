// See https://aka.ms/new-console-template for more information

using Models;
using Qplix.CodeChallenge;
using Qplix.CodeChallenge.Enums;
using Qplix.CodeChallenge.Services;
using Qplix.CodeChallenge.Utils;


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

var line = Console.ReadLine();


while (!string.IsNullOrWhiteSpace(line))
{
    var input = line.Split(Config.CsvDelimiter);
    var date = DateTime.Parse(input[0]);
    var investorId = input[1];
    
    var result = portfolioServices.GetPortfolioValue(date, investorId);
    
    Console.WriteLine($"Total founds {result.TotalFounds.DecimalToMoney()}");
    Console.WriteLine($"Total stocks {result.TotalStocks.DecimalToMoney()}");
    Console.WriteLine($"Total real state {result.TotalRealState.DecimalToMoney()}");
    Console.WriteLine($"Total {(result.TotalFounds + result.TotalStocks + result.TotalRealState).DecimalToMoney()}");

    line = Console.ReadLine();
}
