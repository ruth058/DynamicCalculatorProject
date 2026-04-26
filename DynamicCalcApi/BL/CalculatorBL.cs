using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;
using DynamicExpresso;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace DynamicCalcApi.BL
{

      public class Targil
      {
        public int targil_id { get; set; }
        public string targil { get; set; } = string.Empty;
        public string? tnai { get; set; }
        public string? false_targil { get; set; }
      }

    public static class CalculatorBL
    {

        private static readonly string _connectionString = "Data Source=calculator.db";

        public  static List<object> RunDynamicExpresso()
        {
            Console.WriteLine("\n[API REACHED] Starting calculation...");

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
             // 1. ניקוי תוצאות קודמות של אותה שיטה כדי לא לנפח את הדאטה בייס
            using (var clearCmd = connection.CreateCommand())
            {
                clearCmd.CommandText = "DELETE FROM t_results WHERE method = 'DynamicExpresso'";
                clearCmd.ExecuteNonQuery();
            }

                  // 2. שליפת התרגילים מהטבלה t_targil
            var formulas = new List<Targil>();
            using (var getTasksCmd = connection.CreateCommand())
            {
                getTasksCmd.CommandText = "SELECT * FROM t_targil";
                using var reader = getTasksCmd.ExecuteReader();
                while (reader.Read())
                {
                    formulas.Add(new Targil
                    {
                        targil_id = reader.GetInt32(0),
                        targil = reader.GetString(1),
                        tnai = reader.IsDBNull(2) ? null : reader.GetString(2),
                        false_targil = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }

            var interpreter = new Interpreter();

                interpreter.Reference(typeof(Math)); 
    interpreter.SetFunction("sqrt", (Func<double, double>)Math.Sqrt);
    interpreter.SetFunction("abs", (Func<double, double>)Math.Abs);
    interpreter.SetFunction("log", (Func<double, double>)Math.Log);
    interpreter.SetFunction("pow", (Func<double, double, double>)Math.Pow);
            var summary = new List<object>();

            foreach (var f in formulas)
            {
                int processedRows = 0;

                var parameters = new[] {
                    new Parameter("a", typeof(double)),
                    new Parameter("b", typeof(double)),
                    new Parameter("c", typeof(double)),
                    new Parameter("d", typeof(double))
                };

                   // בניית הלמדות (Lambda expressions) מראש לשיפור ביצועים
                var mainLambda = interpreter.Parse(f.targil, parameters);
                var conditionLambda = !string.IsNullOrEmpty(f.tnai) ? interpreter.Parse(f.tnai, parameters) : null;
                var falseLambda = !string.IsNullOrEmpty(f.false_targil) ? interpreter.Parse(f.false_targil, parameters) : null;
                // שימוש בטרנזקציה להכנסה מהירה של התוצאות
                using var transaction = connection.BeginTransaction();
                using var insertCmd = connection.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = "INSERT INTO t_results (data_id, targil_id, method, result) VALUES (@data_id, @targil_id, 'DynamicExpresso', @result)";
                
                var pDataId = insertCmd.Parameters.Add("@data_id", SqliteType.Integer);
                var pTargilId = insertCmd.Parameters.Add("@targil_id", SqliteType.Integer);
                var pRes = insertCmd.Parameters.Add("@result", SqliteType.Real);
               pTargilId.Value = f.targil_id;

                 using var getDataCmd = connection.CreateCommand();
                getDataCmd.CommandText = "SELECT * FROM t_data";
                using var dataReader = getDataCmd.ExecuteReader();
                var sw = Stopwatch.StartNew();

                while (dataReader.Read())
                {
                    int currentDataId = dataReader.GetInt32(0);
                    
                    double a = dataReader.GetDouble(1);
                    double b = dataReader.GetDouble(2);
                    double c = dataReader.GetDouble(3);
                    double d = dataReader.GetDouble(4);

                    double finalValue = 0;
                    //בדיקה האם יש תנאי למתודה
                    if (conditionLambda != null)
                    {
                       bool isTrue = (bool)conditionLambda.Invoke(a, b, c, d);
                       if (isTrue)//אם עומד בתנאי לוקחים את הנוסחא הרגילה
                            finalValue = Convert.ToDouble(mainLambda.Invoke(a, b, c, d));
                        else
                            finalValue = falseLambda != null ? Convert.ToDouble(falseLambda.Invoke(a, b, c, d)) : 0;
                    }
                     else//אין בנוסחא בכלל תנאי
                    {
                        finalValue = Convert.ToDouble(mainLambda.Invoke(a, b, c, d));
                    }
                    pDataId.Value = currentDataId;
                    pRes.Value = finalValue;

                    insertCmd.ExecuteNonQuery();
                    processedRows++;
                }
                transaction.Commit();
                sw.Stop();

                //רישום בלוג 
                using (var logCmd = connection.CreateCommand())
                {
                    logCmd.CommandText = "INSERT INTO t_log (targil_id, method, run_time) VALUES (@tid, 'DynamicExpresso', @time)";
                    logCmd.Parameters.AddWithValue("@tid", f.targil_id);
                    logCmd.Parameters.AddWithValue("@time", sw.Elapsed.TotalSeconds);
                    logCmd.ExecuteNonQuery();
                }

                  summary.Add(new { 
                    TargilId = f.targil_id, 
                    Targil = f.targil,
                    TimeSeconds = sw.Elapsed.TotalSeconds
                });
            }

           return summary;
        }
    }
}