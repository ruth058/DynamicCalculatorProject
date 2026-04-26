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

namespace DynamicCalcApi.Controllers
{
  
    [ApiController]
    [Route("api/v3/run-calc")]
    public class DataTableCalcController : ControllerBase
   {
     private readonly string _connectionString = "Data Source=calculator.db";
     
        [HttpGet]
        [Route("RunDataTableCalculation")]
        public IActionResult RunDataTableCalculation()
        {
           
            var summary = new List<object>();
            summary =DataTableCalcBL.RunDataTableCalculation();
            return Ok(new { 
             Success = true, 
                Message = "Calculation completed and saved to database.",
                Details = summary
            });
        }

   }
}