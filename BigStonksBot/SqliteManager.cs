using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigStonksBot
{
    class SqliteManager
    {

        public void Drop()
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
DROP TABLE IF EXISTS UserStonk;
DROP TABLE IF EXISTS UserCash
";
                command.ExecuteNonQuery();
            }
        }

        public void Init()
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
CREATE TABLE IF NOT EXISTS UserStonk
(id INTEGER PRIMARY KEY AUTOINCREMENT, user VARCHAR(100), symbol VARCHAR(20), amount REAL, UNIQUE(user, symbol) ON CONFLICT REPLACE)
";
                command.ExecuteNonQuery();
            }
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
CREATE TABLE IF NOT EXISTS UserCash
(id INTEGER PRIMARY KEY AUTOINCREMENT, user VARCHAR(100),  amount REAL, UNIQUE(user) ON CONFLICT REPLACE)
";
                command.ExecuteNonQuery();
            }
        }

        public void UpsertUserStonk(UserStonk userStonk)
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
$@"
INSERT OR IGNORE INTO UserStonk (user, symbol, amount) VALUES (@user, @symbol, @amount); 
UPDATE UserStonk SET amount=@amount WHERE user=@user and symbol=@symbol;
";
                command.Parameters.AddWithValue("@symbol", userStonk.Symbol);
                command.Parameters.AddWithValue("@amount", userStonk.Amount);
                command.Parameters.AddWithValue("@user", userStonk.User);
                command.ExecuteNonQuery();
            }
        }

        public UserStonk GetUserStonk(string symbol, string user)
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
SELECT user, symbol, amount
FROM UserStonk
WHERE symbol = @symbol and user = @user
";
                command.Parameters.AddWithValue("@symbol", symbol);
                command.Parameters.AddWithValue("@user", user);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return new UserStonk
                        {
                            Symbol = symbol,
                            User = user,
                            Amount = 0
                        };
                    }
                    reader.Read();
                    var rowuser = reader.GetString(0);
                    var rowsymbol = reader.GetString(1);
                    var amount = reader.GetDecimal(2);
                    return new UserStonk
                    {
                        User = rowuser,
                        Symbol = rowsymbol,
                        Amount = amount
                    };
                }
            }
        }

        public List<UserStonk> GetUserStonkAll(string user)
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
SELECT user, symbol, amount
FROM UserStonk
WHERE user = @user
";
                command.Parameters.AddWithValue("@user", user);

                List<UserStonk> items = new List<UserStonk>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rowuser = reader.GetString(0);
                        var rowsymbol = reader.GetString(1);
                        var amount = reader.GetDecimal(2);
                        items.Add( new UserStonk
                        {
                            User = rowuser,
                            Symbol = rowsymbol,
                            Amount = amount
                        });
                    }
                }
                return items;
            }
        }

        public List<UserStonk> GetUserStonkAll()
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
SELECT user, symbol, amount
FROM UserStonk
";
                List<UserStonk> items = new List<UserStonk>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var rowuser = reader.GetString(0);
                        var rowsymbol = reader.GetString(1);
                        var amount = reader.GetDecimal(2);
                        items.Add(new UserStonk
                        {
                            User = rowuser,
                            Symbol = rowsymbol,
                            Amount = amount
                        });
                    }
                }
                return items;
            }
        }

        public void UpsertUserCash(string user, decimal cash)
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
$@"
INSERT OR IGNORE INTO UserCash (user, amount) VALUES (@user, @amount); 
UPDATE UserCash SET amount=@amount WHERE user=@user;
";
                command.Parameters.AddWithValue("@amount", cash);
                command.Parameters.AddWithValue("@user", user);
                command.ExecuteNonQuery();
            }
        }

        public decimal GetUserCash(string user)
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
SELECT user, amount
FROM UserCash
WHERE user = @user
";
                command.Parameters.AddWithValue("@user", user);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return 0;
                    }
                    reader.Read();
                    var rowuser = reader.GetString(0);
                    var amount = reader.GetDecimal(1);
                    return amount;
                }
            }
        }

        public bool GetUserCashExists(string user)
        {
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
@"
SELECT user, amount
FROM UserCash
WHERE user = @user
";
                command.Parameters.AddWithValue("@user", user);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
    }
}
