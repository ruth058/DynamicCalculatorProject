using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace DynamicCalcApi
{
    public static class DbInitializer
    {
        public static void Initialize(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            try
            {
                connection.Open();
                Console.WriteLine("--- Starting Database Initialization ---");

                // 1. יצירת הטבלאות אם הן לא קיימות
                CreateTables(connection);

                // 2. הפעלת מפתחות זרים
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA foreign_keys = ON;";
                    cmd.ExecuteNonQuery();
                }

                // 3. הכנסת נתונים (נוסחאות ומידע)
                SeedDataIfEmpty(connection);

                Console.WriteLine("--- Database Initialization Finished ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                throw;
            }
        }

        private static void CreateTables(SqliteConnection conn)
        {
            ExecuteSql(conn, "t_data table", @"
                CREATE TABLE IF NOT EXISTS t_data (
                    data_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    a REAL NOT NULL, b REAL NOT NULL, c REAL NOT NULL, d REAL NOT NULL
                );");

            ExecuteSql(conn, "t_targil table", @"
                CREATE TABLE IF NOT EXISTS t_targil (
                    targil_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    targil TEXT NOT NULL, 
                    tnai TEXT, 
                    targil_false TEXT
                );");

            ExecuteSql(conn, "t_results table", @"
                CREATE TABLE IF NOT EXISTS t_results (
                    result_id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    data_id INTEGER NOT NULL, 
                    targil_id INTEGER NOT NULL, 
                    method TEXT NOT NULL, 
                    result REAL,
                    FOREIGN KEY (data_id) REFERENCES t_data(data_id),
                    FOREIGN KEY (targil_id) REFERENCES t_targil(targil_id)
                );");

            ExecuteSql(conn, "t_log table", @"
                CREATE TABLE IF NOT EXISTS t_log (
                    log_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    targil_id INTEGER NOT NULL, 
                    method TEXT NOT NULL, 
                    run_time REAL,
                    FOREIGN KEY (targil_id) REFERENCES t_targil(targil_id)
                );");
        }

        private static void ExecuteSql(SqliteConnection conn, string description, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            Console.WriteLine($"[OK] {description} checked/created.");
        }

        private static void SeedDataIfEmpty(SqliteConnection conn)
        {
            // בדיקה והכנסת נוסחאות לטבלת התרגילים
            long targilCount = GetCount(conn, "t_targil");
            if (targilCount == 0)
            {
                Console.WriteLine("t_targil is empty. Inserting formulas from images...");
                using var cmd = conn.CreateCommand();
                
                // הכנסת כל הנוסחאות מהתמונות + הנוסחה עם התנאי שביקשת
                cmd.CommandText = @"
                    -- נוסחאות פשוטות
                    INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('a + b', NULL, NULL);
                    INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('c * 2', NULL, NULL);
                    INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('b - a', NULL, NULL);
                    INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('d / 4', NULL, NULL);

      -- נוסחאות מורכבות
      INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('(a + b) * 8', NULL, NULL);
       INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('sqrt(pow(c, 2) + pow(d, 2))', NULL, NULL);
       INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('log(b) + c', NULL, NULL);
       INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('abs(d - b)', NULL, NULL);

                    -- נוסחה עם תנאי (לבקשתך)
                    INSERT INTO t_targil (targil, tnai, targil_false) VALUES ('a + b', 'a > 50', 'b * 2');";
                
                cmd.ExecuteNonQuery();
                Console.WriteLine("[SUCCESS] All formulas inserted into t_targil.");
            }
            else
            {
                Console.WriteLine($"t_targil already has {targilCount} rows. Skipping formula insert.");
            }

            // בדיקה והכנסת מיליון רשומות לטבלת הנתונים
            long dataCount = GetCount(conn, "t_data");
            if (dataCount == 0)
            {
                Console.WriteLine("t_data is empty. Inserting 1,000,000 rows...");
                using var transaction = conn.BeginTransaction();
                using var insertCmd = conn.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = "INSERT INTO t_data (a, b, c, d) VALUES (@a, @b, @c, @d)";
                
                var pa = insertCmd.Parameters.Add("@a", SqliteType.Real);
                var pb = insertCmd.Parameters.Add("@b", SqliteType.Real);
                var pc = insertCmd.Parameters.Add("@c", SqliteType.Real);
                var pd = insertCmd.Parameters.Add("@d", SqliteType.Real);

                Random rnd = new Random();
                for (int i = 0; i < 1000000; i++)
                {
                    pa.Value = Math.Round(rnd.NextDouble() * 100, 2);
                    pb.Value = Math.Round(rnd.NextDouble() * 100, 2);
                    pc.Value = Math.Round(rnd.NextDouble() * 100, 2);
                    pd.Value = Math.Round(rnd.NextDouble() * 100, 2);
                    insertCmd.ExecuteNonQuery();

                    if ((i + 1) % 250000 == 0) Console.WriteLine($"Progress: {i + 1} rows...");
                }
                transaction.Commit();
                Console.WriteLine("[SUCCESS] 1,000,000 rows added to t_data.");
            }
            else
            {
                Console.WriteLine($"t_data already has {dataCount} rows. Skipping data insert.");
            }
        }

        private static long GetCount(SqliteConnection conn, string tableName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            return (long)(cmd.ExecuteScalar() ?? 0);
        }
    }
}