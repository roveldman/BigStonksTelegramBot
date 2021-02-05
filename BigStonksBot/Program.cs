using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace BigStonksBot
{
    class Program
    {
        private static SqliteManager sqliteManager;
        static TelegramBotClient bc;
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        static async Task Main(string[] args)
        {
            sqliteManager = new SqliteManager();
            // sqliteManager.Drop();
            sqliteManager.Init();

            var telegramKey = System.IO.File.ReadAllText("./telegram.txt").Trim();
            bc = new TelegramBotClient(telegramKey);
            bc.OnMessage += Bc_OnMessage;
            bc.StartReceiving();

            // Handle Control+C or Control+Break
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");

                // Allow the manin thread to continue and exit...
                waitHandle.Set();
            };

            // Wait
            waitHandle.WaitOne();
        }

        private static async void Bc_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var array = e.Message.Text.Split(" ");
                string user = e.Message.From.Id.ToString();

                var hasEntry = sqliteManager.GetUserCashExists(user);
                if (!hasEntry)
                {
                    decimal beginAmt = 1000;
                    sqliteManager.UpsertUserCash(user, beginAmt);

                    await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: $"Welcome {e.Message.From.FirstName}!\n" +
                                                                                   $"Your account has been set up with {beginAmt} USD.");
                }
                

                if (array.Count() == 2 && array[0].Equals("/price"))
                {
                    var symbol = array[1].ToUpper();

                    // Get price for stock
                    var price = (await StonkManager.Get(symbol)).LatestPrice;

                    // Write result
                    Message pollMessage = await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: symbol + ": " + price.ToString());
                }

                if (array.Count() == 3 && array[0].Equals("/buy"))
                {
                    var symbol = array[1].ToUpper();
                    var shares = decimal.Parse(array[2]);

                    // Get stock price
                    var price = (await StonkManager.Get(symbol)).LatestPrice;
                    var cost = shares * price;

                    // Get current user stock entry
                    var amountBefore = sqliteManager.GetUserStonk(symbol, user).Amount;

                    // Check money
                    var beforeCash = sqliteManager.GetUserCash(user);

                    // Remove money
                    var newBalance = beforeCash - cost;

                    if (newBalance < 0)
                    {
                        string cashMsg = $"{e.Message.From.FirstName}, You need ${-newBalance} dollars for that.";
                        Message notEnoughCashMessage = await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: cashMsg);
                        return;
                    }

                    sqliteManager.UpsertUserCash(user, newBalance);

                    // Add the stock
                    sqliteManager.UpsertUserStonk(new UserStonk
                    {
                        User = user,
                        Amount = amountBefore + shares,
                        Symbol = symbol
                    });

                    // Lookup stock entry again
                    var userStonkUsperted = sqliteManager.GetUserStonk(symbol, user);
                    var userCashUpserted = sqliteManager.GetUserCash(user);

                    // Print result
                    var message = $"{e.Message.From.FirstName}, you now have {userStonkUsperted.Amount} of symbol {userStonkUsperted.Symbol}.\nCost: {cost} USD\nBank Balance: {userCashUpserted} USD.";
                    Message pollMessage = await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: message);
                }
                if (array.Count() == 3 && array[0].Equals("/sell"))
                {
                    var symbol = array[1].ToUpper();
                    var shares = decimal.Parse(array[2]);

                    // Get stock price
                    var price = (await StonkManager.Get(symbol)).LatestPrice;
                    var cost = shares * price;

                    // Ensure the user has enough of the stock
                    var beforeStonks = sqliteManager.GetUserStonk(symbol, user);
                    if (beforeStonks.Amount < shares)
                    {
                        string stonkMsg = "You don't have that many shares";
                        await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: stonkMsg);
                        return;
                    }
                    
                    // Remove the stock
                    sqliteManager.UpsertUserStonk(new UserStonk
                    {
                        User = user,
                        Amount = beforeStonks.Amount - shares,
                        Symbol = symbol
                    });

                    // Add the cash
                    var amountBefore = sqliteManager.GetUserCash(user);
                    decimal newBalance = amountBefore + cost;
                    sqliteManager.UpsertUserCash(user, newBalance);

                    var amountAfter = sqliteManager.GetUserCash(user);
                    var stonkAmountAfter = sqliteManager.GetUserStonk(symbol, user);

                    var message = $"You now have {stonkAmountAfter.Amount} of {stonkAmountAfter.Symbol}.\nYou have {amountAfter} USD.";
                    Message pollMessage = await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: message);
                }

                if (array.Count() == 1 && array[0].Equals("/me"))
                {
                    StringBuilder builder = new StringBuilder();
                    var stonks = sqliteManager.GetUserStonkAll(user);

                    builder.AppendLine("Your Stonks:");
                    foreach (var stonk in stonks)
                    {
                        builder.AppendLine($"{stonk.Symbol}: {stonk.Amount}");
                    }

                    if (!stonks.Any())
                    {
                        builder.AppendLine("None");
                    }

                    var cash = sqliteManager.GetUserCash(user);
                    builder.AppendLine("Cash: " + cash);

                    await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: builder.ToString());
                }

                if (array.Count() == 1 && array[0].Equals("/leaderboard"))
                {
                    StringBuilder builder = new StringBuilder();
                    var allStonks = sqliteManager.GetUserStonkAll();
                    var users = allStonks.Select(x => x.User).ToHashSet();

                    var symbols = allStonks.Where(x => x.Amount != 0).Select(x => x.Symbol).ToHashSet();

                    var prices = symbols.ToDictionary(x => x, x => (StonkManager.Get(x).Result).LatestPrice);

                    List<LeaderBoardResult> results = new List<LeaderBoardResult>();
                    foreach (var userId in users)
                    {
                        var userAssets = allStonks.Where(x => x.User.Equals(userId));
                        var userSum = userAssets.Where(x => prices.ContainsKey(x.Symbol)).Select(x => prices[x.Symbol] * x.Amount).Sum();
                        var userObject = await bc.GetChatMemberAsync(e.Message.Chat.Id, int.Parse(userId));
                        results.Add(new LeaderBoardResult
                        {
                            Cash = sqliteManager.GetUserCash(userId),
                            Name = userObject.User.FirstName + " " + userObject.User.LastName,
                            Stocks = userSum,
                            UserId = userId
                        });
                    }

                    builder.AppendLine("Leaderboard (Cash, Stocks)");
                    results = results.OrderByDescending(x => x.Stocks + x.Cash).ToList();
                    for (int i = 0; i < results.Count; i++)
                    {
                        var result = results[i];
                        builder.AppendLine($"{i + 1}.) {result.Name}: ({result.Cash + result.Stocks} USD)");
                    }

                    await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: builder.ToString());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await bc.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: ":/ I didn't get that.");
            }
        }
    }
}
