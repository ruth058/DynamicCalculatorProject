using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using DynamicCalcApi.BL;

namespace DynamicCalcApi.Controllers
{
    [ApiController]
    [Route("api/v2/run-calc")] 
    public class SqlDynamicController : ControllerBase
    {

        [HttpGet("run-sql-dynamic")]
        public IActionResult RunSqlDynamic()
        {
            var resultsSummary = new List<object>();

            resultsSummary=SqlDynamicBL.RunSqlDynamic();
            if(resultsSummary==null)
                return StatusCode(500, $"Internal error:");

            return Ok(new { 
                Success = true, 
                Message = "Calculation completed and saved to database.",
                Details = resultsSummary
            });
        }
    }

 
}