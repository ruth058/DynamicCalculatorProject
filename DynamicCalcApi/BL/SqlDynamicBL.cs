using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace DynamicCalcApi.BL
{
  public static class SqlDynamicBL
  {
        private static readonly string _connectionString = "Data Source=calculator.db";

//פונקציה זו עוברת על כל תרגיל ובונה שאילתא דינאמית בהתאם לנתונים
        public static List<object> RunSqlDynamic()
        {
            var resultsSummary = new List<object>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using (var walCmd = new SqliteCommand("PRAGMA journal_mode = WAL;", connection))
            {
                walCmd.ExecuteNonQuery();
            }
            var formulas = new List<Targil>();



            // שליפת רשימת התרגילים 
            using (var cmd = new SqliteCommand("SELECT * FROM t_targil", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    formulas.Add(new Targil {
                        targil_id = reader.GetInt32(0),
                        targil = reader.GetString(1),
                        tnai = reader.IsDBNull(2) ? null : reader.GetString(2),
                        false_targil = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }
            using var transaction = connection.BeginTransaction();

            try
            {
                //מעבר על כל התרגילים 
                foreach (var f in formulas)
                {
                 string dynamicSql;
                 var totalSw = Stopwatch.StartNew();
                   if (!string.IsNullOrEmpty(f.tnai))//אם יש תנאי לנוסחא
                    {
                           dynamicSql = $@"
                            INSERT INTO t_results (data_id, targil_id, method, result)
                            SELECT data_id, {f.targil_id}, 'SQL_Dynamic', 
                            CASE WHEN {f.tnai} THEN ({f.targil}) ELSE ({f.false_targil ?? "0"}) END
                            FROM t_data";
                    }
                    else
                    {
                          dynamicSql = $@"
                            INSERT INTO t_results (data_id, targil_id, method, result)
                            SELECT data_id, {f.targil_id}, 'SQL_Dynamic', ({f.targil})
                            FROM t_data";
                    }

//הרצת השאילתא הדינאמית שנוצרה בהתאם לתנאים
                    using (var calcCmd = new SqliteCommand(dynamicSql, connection, transaction))
                    {
                        calcCmd.ExecuteNonQuery();
                    }
                    totalSw.Stop(); 

                      // רישום לוג עבור התרגיל הנוכחי 
                    using (var logCmd = new SqliteCommand("INSERT INTO t_log (targil_id, method, run_time) VALUES (@tid, 'SQL_Dynamic', @time)", connection, transaction))
                    {
                        logCmd.Parameters.AddWithValue("@tid", f.targil_id);
                        logCmd.Parameters.AddWithValue("@time", totalSw.Elapsed.TotalSeconds);
                        logCmd.ExecuteNonQuery();
                    }


                    
                        resultsSummary.Add(new {
                        TargilId = f.targil_id,
                        Targil=f.targil,
                        TimeSeconds = totalSw.Elapsed.TotalSeconds
                    });
                }
                 // שמירה סופית של הכל 
                transaction.Commit();
               
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"[ERROR] Calculation failed: {ex.Message}");
                return new List<object>();
            }


            return resultsSummary;
        }

  }

    
}