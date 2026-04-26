using Microsoft.AspNetCore.Mvc;
using DynamicCalcApi.BL;

namespace DynamicCalcApi.Controllers
{
    [ApiController]
    [Route("api/MainCalcController")]
    public class MainCalcController : ControllerBase
    {
        //פונקציה שקוראת לכל שלושת השיטות
        [HttpGet("run-all")]
        public IActionResult runAll()
        {
            try
            {
                List<object> summaryV1 = CalculatorBL.RunDynamicExpresso();

                List<object> summaryV2 = SqlDynamicBL.RunSqlDynamic();
               
                List<object> summaryV3 = DataTableCalcBL.RunDataTableCalculation();

                return Ok(new 
                { 
                    Message = "Sequence completed successfully.",
                    summaryV1 = summaryV1,
                    summaryV2 = summaryV2,
                    summaryV3 = summaryV3
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Sequence interrupted: {ex.Message}");
            }
        }
    }
}