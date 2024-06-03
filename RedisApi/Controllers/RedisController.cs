using Microsoft.AspNetCore.Mvc;

namespace RedisApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly TableService _tableService;

        public RedisController(TableService tableService)
        {
            _tableService = tableService;
        }

        [HttpPost("insert")]
        public IActionResult InsertRecord([FromBody] Dictionary<string, object> data, [FromQuery] int id, [FromQuery] string tableName)
        {
            bool success = _tableService.InsertTableRow(tableName, id, data);
            if (success)
                return Ok("Record inserted successfully.");
            else
                return StatusCode(500, "Error inserting the record.");
        }

        [HttpGet("get-entry")]
        public IActionResult GetField([FromQuery] string tableName, [FromQuery] int id, [FromQuery] string fieldName = "")
        {
            var result = _tableService.GetEntryOrField(tableName, id, fieldName);
            if (result != null && result.Count > 0)
            {
                return Ok(result);
            }
            else if (!string.IsNullOrEmpty(fieldName))
            {
                return NotFound($"Field '{fieldName}' not found in '{tableName}' with ID '{id}'.");
            }
            else
            {
                return NotFound($"No entry found for '{tableName}' with ID '{id}'.");
            }
        }


        [HttpGet("get-all-tables")]
        public async Task<IActionResult> GetAllTables()
        {
            var tableNames = await _tableService.GetAllTableNames();
            if (tableNames.Any())
            {
                return Ok(tableNames);
            }
            else
            {
                return NotFound("No tables found.");
            }
        }


        [HttpGet("get-all-entries")]
        public async Task<IActionResult> GetAllEntries([FromQuery] string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return BadRequest("Table name must be provided.");
            }

            var entries = await _tableService.GetAllEntriesForTable(tableName);
            if (entries.Count > 0)
            {
                return Ok(entries);
            }
            else
            {
                return NotFound($"No entries found for table '{tableName}'.");
            }
        }

        [HttpDelete("delete-entry")]
        public IActionResult DeleteEntry([FromQuery] string tableName, [FromQuery] int id)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return BadRequest("Table name must be provided.");
            }

            bool deleted = _tableService.DeleteEntry(tableName, id);
            if (deleted)
            {
                return Ok($"Entry deleted successfully from table '{tableName}' with ID '{id}'.");
            }
            else
            {
                return NotFound($"No entry found for table '{tableName}' with ID '{id}'.");
            }
        }

    }
}
