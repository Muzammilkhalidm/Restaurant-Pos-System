using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using RestaurantPOS.Models;

namespace RestaurantPOS.Data
{
    /// <summary>
    /// Handles all SQLite database access for the POS system.
    /// The database file is created automatically on first run under
    /// %LocalAppData%\RestaurantPOS\restaurant_pos.db — no server install required.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string DbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RestaurantPOS");
        private static readonly string DbPath = Path.Combine(DbFolder, "restaurant_pos.db");

        public static string ConnectionString => $"Data Source={DbPath}";

        public static void Initialize()
        {
            Directory.CreateDirectory(DbFolder);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            const string createTables = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    Role TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS MenuItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Price REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Orders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerName TEXT NOT NULL,
                    OrderDate TEXT NOT NULL,
                    TotalAmount REAL NOT NULL,
                    CashierUsername TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS OrderDetails (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    MenuItemId INTEGER NOT NULL,
                    ItemName TEXT NOT NULL,
                    Price REAL NOT NULL,
                    Quantity INTEGER NOT NULL,
                    LineTotal REAL NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
                );";

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = createTables;
                cmd.ExecuteNonQuery();
            }

            SeedIfEmpty(connection);
            EnsureAdminUser(connection);
        }

        /// <summary>
        /// Guarantees the protected Admin Panel account exists, even on a database that
        /// was created before this account was introduced. Safe to run every startup.
        /// </summary>
        private static void EnsureAdminUser(SqliteConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT OR IGNORE INTO Users (Username, Password, Role) VALUES ('muzammil', 'muzammil182921', 'SuperAdmin')";
            cmd.ExecuteNonQuery();
        }

        private static void SeedIfEmpty(SqliteConnection connection)
        {
            using (var checkUsers = connection.CreateCommand())
            {
                checkUsers.CommandText = "SELECT COUNT(*) FROM Users";
                var count = Convert.ToInt32(checkUsers.ExecuteScalar());
                if (count == 0)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText =
                        "INSERT INTO Users (Username, Password, Role) VALUES " +
                        "('admin', 'admin123', 'Manager'), ('cashier', 'cashier123', 'Cashier')";
                    cmd.ExecuteNonQuery();
                }
            }

            using (var checkMenu = connection.CreateCommand())
            {
                checkMenu.CommandText = "SELECT COUNT(*) FROM MenuItems";
                var count = Convert.ToInt32(checkMenu.ExecuteScalar());
                if (count == 0)
                {
                    foreach (var item in SeedData.GetMenuItems())
                    {
                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = "INSERT INTO MenuItems (Name, Category, Price) VALUES ($name, $category, $price)";
                        cmd.Parameters.AddWithValue("$name", item.Name);
                        cmd.Parameters.AddWithValue("$category", item.Category);
                        cmd.Parameters.AddWithValue("$price", item.Price);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // ---------------- Authentication ----------------

        public static bool ValidateUser(string username, string password, out string role)
        {
            role = null;
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Role FROM Users WHERE Username = $u AND Password = $p";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$p", password);
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                role = result.ToString();
                return true;
            }
            return false;
        }

        // ---------------- Menu ----------------

        public static List<MenuItem> GetAllMenuItems()
        {
            var items = new List<MenuItem>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Category, Price FROM MenuItems ORDER BY Category, Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new MenuItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Price = Convert.ToDecimal(reader.GetDouble(3))
                });
            }
            return items;
        }

        public static List<string> GetCategories()
        {
            var categories = new List<string> { "All" };
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT Category FROM MenuItems ORDER BY Category";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                categories.Add(reader.GetString(0));
            return categories;
        }

        public static void AddMenuItem(MenuItem item)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO MenuItems (Name, Category, Price) VALUES ($name, $category, $price)";
            cmd.Parameters.AddWithValue("$name", item.Name);
            cmd.Parameters.AddWithValue("$category", item.Category);
            cmd.Parameters.AddWithValue("$price", item.Price);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateMenuItem(MenuItem item)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE MenuItems SET Name=$name, Category=$category, Price=$price WHERE Id=$id";
            cmd.Parameters.AddWithValue("$name", item.Name);
            cmd.Parameters.AddWithValue("$category", item.Category);
            cmd.Parameters.AddWithValue("$price", item.Price);
            cmd.Parameters.AddWithValue("$id", item.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteMenuItem(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM MenuItems WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ---------------- Orders ----------------

        public static int SaveOrder(string customerName, string cashier, decimal total, List<OrderItem> cart)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            int orderId;
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText =
                    "INSERT INTO Orders (CustomerName, OrderDate, TotalAmount, CashierUsername) " +
                    "VALUES ($c,$d,$t,$u); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("$c", customerName);
                cmd.Parameters.AddWithValue("$d", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("$t", total);
                cmd.Parameters.AddWithValue("$u", cashier);
                orderId = Convert.ToInt32((long)cmd.ExecuteScalar());
            }

            foreach (var line in cart)
            {
                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText =
                    "INSERT INTO OrderDetails (OrderId, MenuItemId, ItemName, Price, Quantity, LineTotal) " +
                    "VALUES ($oid,$mid,$name,$price,$qty,$lt)";
                cmd.Parameters.AddWithValue("$oid", orderId);
                cmd.Parameters.AddWithValue("$mid", line.MenuItemId);
                cmd.Parameters.AddWithValue("$name", line.Name);
                cmd.Parameters.AddWithValue("$price", line.Price);
                cmd.Parameters.AddWithValue("$qty", line.Quantity);
                cmd.Parameters.AddWithValue("$lt", line.Total);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return orderId;
        }

        public static (int totalOrders, decimal totalSales, int totalItems) GetDashboardStats()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            int totalOrders = 0;
            decimal totalSales = 0;
            int totalItems;

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*), IFNULL(SUM(TotalAmount),0) FROM Orders";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    totalOrders = reader.GetInt32(0);
                    totalSales = Convert.ToDecimal(reader.GetDouble(1));
                }
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM MenuItems";
                totalItems = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return (totalOrders, totalSales, totalItems);
        }

        /// <summary>
        /// Orders placed today only (used for the "Today" cards on the dashboard).
        /// </summary>
        public static (int todayOrders, decimal todaySales) GetTodayStats()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*), IFNULL(SUM(TotalAmount),0) FROM Orders WHERE substr(OrderDate,1,10) = $today";
            cmd.Parameters.AddWithValue("$today", DateTime.Now.ToString("yyyy-MM-dd"));
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return (reader.GetInt32(0), Convert.ToDecimal(reader.GetDouble(1)));
            return (0, 0);
        }

        /// <summary>
        /// Sales totals grouped by calendar day, most recent day first.
        /// </summary>
        public static List<(string Date, int OrderCount, decimal TotalSales)> GetDailySales()
        {
            var results = new List<(string, int, decimal)>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT substr(OrderDate,1,10) AS Day, COUNT(*), SUM(TotalAmount) " +
                "FROM Orders GROUP BY Day ORDER BY Day DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add((reader.GetString(0), reader.GetInt32(1), Convert.ToDecimal(reader.GetDouble(2))));
            }
            return results;
        }

        /// <summary>
        /// All individual orders, most recent first. Used by the Admin Panel so a single
        /// order can be selected and deleted without touching any other sales data.
        /// </summary>
        public static List<OrderSummary> GetAllOrders()
        {
            var orders = new List<OrderSummary>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, CustomerName, OrderDate, TotalAmount, CashierUsername FROM Orders ORDER BY OrderDate DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                orders.Add(new OrderSummary
                {
                    Id = reader.GetInt32(0),
                    CustomerName = reader.GetString(1),
                    OrderDate = DateTime.Parse(reader.GetString(2)),
                    TotalAmount = Convert.ToDecimal(reader.GetDouble(3)),
                    CashierUsername = reader.GetString(4)
                });
            }
            return orders;
        }

        /// <summary>
        /// Deletes exactly one order (and its line items). All other sales are untouched.
        /// </summary>
        public static void DeleteOrder(int orderId)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM OrderDetails WHERE OrderId = $id";
                cmd.Parameters.AddWithValue("$id", orderId);
                cmd.ExecuteNonQuery();
            }
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM Orders WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", orderId);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        /// <summary>
        /// Wipes ALL sales history (every order and order line). Menu items are untouched.
        /// Only reachable through the Admin Panel.
        /// </summary>
        public static void ClearAllSales()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "DELETE FROM OrderDetails; DELETE FROM Orders; " +
                                   "DELETE FROM sqlite_sequence WHERE name IN ('Orders','OrderDetails');";
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
