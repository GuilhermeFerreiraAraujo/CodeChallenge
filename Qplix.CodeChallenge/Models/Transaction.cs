using System;
using System.Globalization;
using Qplix.CodeChallenge.Enums;
using Qplix.CodeChallenge.Interfaces;
using Qplix.CodeChallenge.Utils;

namespace Models
{
    public class Transaction : IEntity
    {
        public string InvestmentId { get; set; }
        public TransactionTypes Type { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public void ParseLine(string line)
        {
            var columns = line.Split(Config.CsvDelimiter);

            InvestmentId = columns[0];
            Type = Enum.Parse<TransactionTypes>(columns[1]);
            Date = DateTime.Parse(columns[2]);
            Value = columns[3].StringToDecimal();
        }
    }
}