using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BigStonksBot
{
    class StonkManager
    {
        public static string secretToken = File.ReadAllText("iex.txt");
        public static async Task<QuoteIEX> Get(string symbol)
        {
            HttpClient client = new HttpClient();
            var quoteResponse = await client.GetAsync($"https://cloud.iexapis.com/stable/stock/{symbol}/quote?token={secretToken}");
            var quoteJSON = await quoteResponse.Content.ReadAsStringAsync();

            QuoteIEX f = JsonConvert.DeserializeObject<QuoteIEX>(quoteJSON);
            return f;
        }
    }
}
