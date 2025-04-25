using Qplix.CodeChallenge;
using Qplix.CodeChallenge.Enums;
using Qplix.CodeChallenge.Interfaces;

namespace Models
{
    
    public class Investment : IEntity
    {
        public string InvestorId { get; set; }
        public string InvestmentId { get; set; }
        public InvestmentTypes InvestmentType { get; set; }
        public string Isin { get; set; }
        public string City { get; set; }
        public string FoundsInvestor { get; set; }
        
        public void ParseLine(string line)
        {
            var columns = line.Split(Config.CsvDelimiter);
                
            InvestorId = columns[0];
            InvestmentId = columns[1];
            InvestmentType = Enum.Parse<InvestmentTypes>(columns[2]);
            Isin = columns[3];
            City = columns[4];
            FoundsInvestor = columns[5];        }
    }
}