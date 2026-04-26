using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Data;
using DynamicCalcApi.BL;
using NCalc;


namespace DynamicCalcApi.BL
{
    public class t_data { 
        public int data_id; 
        public double a, b, c, d; 
    }
    public class t_results { 
        public int data_id; 
        public int targil_id; 
        public double result; 
    }
  public static class DataTableCalcBL
  {
        private static string _connectionString = "Data Source=calculator.db;Cache=Shared;Mode=ReadWriteCreate;";

    public static List<object> RunDataTableCalculation()
        {
            var summary = new List<object>();
            
            // שימוש בחיבור אחד בלבד לאורך כל התהליך
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var walCmd = new SqliteCommand("PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;", connection))
                {
                    walCmd.ExecuteNonQuery();
                }

                //  שליפת כל התרגילים מהטבלה t_targil
                var formulas = new List<dynamic>();
                using (var getTasksCmd = new SqliteCommand("SELECT * FROM t_targil", connection))
                {
                    using (var reader = getTasksCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            formulas.Add(new {
                                targil_id = reader.GetInt32(0),
                                targil = reader.GetString(1),
                                tnai = reader.IsDBNull(2) ? null : reader.GetString(2),
                                targil_false = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                        }
                    }
                }

                // שליפת כל נתוני הdata 
                DataTable dt_data = new DataTable();
                using (var getDataCmd = new SqliteCommand("SELECT data_id, a, b, c, d FROM t_data", connection))
                {
                    using (var dataReader = getDataCmd.ExecuteReader())
                    {
                        dt_data.Load(dataReader); 
                    }
                }

                // מעבר בלולאה על כל נוסחא
                foreach (var f in formulas)
                {
                    //תחילת חישוב זמן הריצה
                    var taskSw = Stopwatch.StartNew();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try 
                        {
                            using (var insertCmd = connection.CreateCommand())
                            {
                                insertCmd.Transaction = transaction;
                                insertCmd.CommandText = "INSERT INTO t_results (data_id, targil_id, method, result) VALUES (@did, @tid, 'DataTable', @res)";
                                
                                var pDid = insertCmd.Parameters.Add("@did", SqliteType.Integer);
                                var pTid = insertCmd.Parameters.Add("@tid", SqliteType.Integer);
                                var pRes = insertCmd.Parameters.Add("@res", SqliteType.Real);
                                
                                pTid.Value = f.targil_id;

                                // בניית הנוסחה: אם יש תנאי, נשתמש בפורמט if(condition, true, false)
                              string formulaToEvaluate = !string.IsNullOrEmpty(f.tnai)  ? $"if({f.tnai}, {f.targil}, {f.targil_false ?? "0"})" : f.targil;

                              formulaToEvaluate = formulaToEvaluate.Replace("log(", "Log10(", StringComparison.OrdinalIgnoreCase);

                                // יצירת אובייקט החישוב של NCalc
                                //השתמשתי בזה היות ובDataTable.compute איו אפשרות לחישוב נוסחאות מורכבות 
                               var ncalcExpr = new Expression(formulaToEvaluate, EvaluateOptions.IgnoreCase);
                                // מעבר על כל הדתה 
                                foreach (DataRow row in dt_data.Rows)
                                {
                                    // הזנת הפרמטרים מהשורה הנוכחית כפרמטרים לנוסחא 
                                    ncalcExpr.Parameters["a"] = row["a"];
                                    ncalcExpr.Parameters["b"] = row["b"];
                                    ncalcExpr.Parameters["c"] = row["c"];
                                    ncalcExpr.Parameters["d"] = row["d"];

                                    // ביצוע החישוב באופן דינאמי
                                    object result = ncalcExpr.Evaluate();
                                    
                                    // עדכון ערכי הפרמטרים ב-SQL ושמירה
                                    pDid.Value = row["data_id"];
                                    pRes.Value = Convert.ToDouble(result ?? 0);
                                    
                                    insertCmd.ExecuteNonQuery();
                                }
                            }

                            // רישום זמן הביצוע בטבלת הלוג
                            using (var logCmd = connection.CreateCommand())
                            {
                                logCmd.Transaction = transaction;
                                logCmd.CommandText = "INSERT INTO t_log (targil_id, method, run_time) VALUES (@tid, 'DataTable', @time)";
                                logCmd.Parameters.AddWithValue("@tid", f.targil_id);
                                logCmd.Parameters.AddWithValue("@time", taskSw.Elapsed.TotalSeconds);
                                logCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            taskSw.Stop();

                       
                summary.Add(new { 
                  TargilId = f.targil_id,
                        Targil=f.targil,
                        TimeSeconds = taskSw.Elapsed.TotalSeconds
                });
                        }
                        catch (Exception ex)
                        {
                            // במקרה של שגיאה בנוסחה ספציפית, נבטל רק אותה ונמשיך לאחרות
                            transaction.Rollback();
                            Debug.WriteLine($"Error in Formula {f.targil_id}: {ex.Message}");
                            
                            summary.Add(new { 
                                TargilId = f.targil_id,
                                Status = "Error",
                                Message = ex.Message
                            });
                        }
                    }
                }
            } 

            return summary;
        }
  }
}