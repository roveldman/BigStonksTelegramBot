using Newtonsoft.Json;

namespace BigStonksBot
{
    internal class QuoteIEX
    {
        public string Symbol { get; set; }
        public decimal LatestPrice { get; set; }
        public string CompanyName { get; set; }
    }
}