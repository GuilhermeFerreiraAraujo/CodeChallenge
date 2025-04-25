namespace Models.Responses;

public class StockQuote
{
    public string Isin { get; set; }
    
    public decimal Value { get; set; }
}