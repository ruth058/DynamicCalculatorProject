using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Globalization;
using System.Data;
using DynamicExpresso;
using DynamicCalcApi.BL;

namespace DynamicCalcApi.Controllers
{
  

    [ApiController]
    [Route("api/v1/run-calc")] 
     public  class CalculatorController : ControllerBase
    {
        [HttpGet]
        [Route("run-dynamic-expresso")]

        public  IActionResult RunDynamicExpresso()
        {
            var summary = new List<object>();
            summary=CalculatorBL.RunDynamicExpresso();
      
            return Ok(new { 
                Message = "Calculation completed and saved to database.",
                Summary = summary 
            });
        }
    }
}
