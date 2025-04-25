// See https://aka.ms/new-console-template for more information

using Models;
using Qplix.CodeChallenge;
using Qplix.CodeChallenge.Services;
using Qplix.CodeChallenge.Utils;

var investmentFileReader = new FileReader<Investment>(Config.FilePath,"Investments.csv");
var transactionFileReader = new FileReader<Transaction>(Config.FilePath,"Transactions.csv");
var quoteFileReader = new FileReader<Quote>(Config.FilePath,"Quotes.csv");  

var investments = investmentFileReader.ReadFile();
var transactions = transactionFileReader.ReadFile();
var quotes = quoteFileReader.ReadFile().ToList();

var percentages = transactions.Where(x => x.Type == "Percentage").ToList();
var shares = transactions.Where(x => x.Type == "Shares").ToList();
var foundsInvestments = investments.Where(x => x.InvestorId.Contains("Fonds")).ToList();




var portfolioServices = new PortfolioServices(transactions, quotes, investments, percentages, shares, foundsInvestments);

var line = Console.ReadLine();


while (!string.IsNullOrWhiteSpace(line))
{
    
    
    var input = line.Split(Config.CsvDelimiter);
    var date = DateTime.Parse(input[0]);
    var investorId = input[1];
    
    var result = portfolioServices.GetTotalFoundsValue(date, investorId);
    
    //var result = portfolioServices.GetPortfolioValue(date, investorId);
    
    line = Console.ReadLine();
    
}



Console.WriteLine("Hello, World!");
