using System;
using Qplix.CodeChallenge.Interfaces;
using Qplix.CodeChallenge.Utils;

namespace Models
{
    public class Quote : IEntity
    {
        public string Isin { get; set; }
        public DateTime Date { get; set; }
        public decimal PricePerShare { get; set; }
        public void ParseLine(string line)
        {
            var columns = line.Split(Config.CsvDelimiter);
            Isin = columns[0];
            Date = DateTime.Parse(columns[1]);
            PricePerShare = columns[2].StringToDecimal();

        }
    }
}