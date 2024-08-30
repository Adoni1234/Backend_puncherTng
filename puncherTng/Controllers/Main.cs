using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using puncherTng.ContextDB;
using puncherTng.Models;
using System.Threading.Tasks;

namespace puncherTng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Main : ControllerBase
    {
        private readonly Context _context;

        public Main(Context context)
        {
            _context = context;
        }

        [HttpPost("access")]
        public async Task<IActionResult> AgentAccess(InputAccess data)
        {
            if (data == null)
                return BadRequest("Data is required");
            if (string.IsNullOrEmpty(data.code))
                return BadRequest("Code is required");

            var nameEmployee = await SearchingForemployeeName(data.code);
            if (nameEmployee == null || nameEmployee.Length == 0 || nameEmployee[0] == null)
            {
                return Ok(new { message = "Error: Employee not found or invalid code" });
            }
            if (nameEmployee[1] == "Inactivo" || nameEmployee[1] == "")
                return BadRequest("Su codigo de validacion esta innactivo");

            var validCountPuncher = await ValidByHoursPuncher(data.code);

            if(validCountPuncher >= 1)
            {
                return Ok(new { message = "Error: Ya has ponchador la cantida de veces requeridas por dia" });

            }





            try
            {
                string sql = "INSERT INTO accesses (code) VALUES (@Code); SELECT CAST(scope_identity() AS int);";

                SqlParameter[] parameters =
                {
                   new SqlParameter("@Code", data.code),
                };
                var insertedId = await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                var insertedData = await _context.accesses.FindAsync(insertedId);
                if (insertedData == null)
                    return BadRequest("Error retrieving inserted data");

                return Ok(new
                {
                    message = "Valid code",
                    data = insertedData,
                    name = nameEmployee[0]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error saving data to the database: " + ex.Message);
            }
        }

        [HttpGet("employee-name")]
        public async Task<string[]> SearchingForemployeeName(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var accessCode = await _context.accessCodes
                                           .FirstOrDefaultAsync(a => a.code == code);
            if (accessCode == null)
                return null;

            var employee = await _context.agentes
                                         .FirstOrDefaultAsync(e => e.Id == accessCode.IdAgente);
            if (employee == null)
                return null;

            return [employee.Name, employee.status];
        }


        [HttpGet("day/poncher")]
        public async Task<int> ValidByHoursPuncher(string code)
        {
            if (string.IsNullOrEmpty(code))
                return 0;

            var accessCode = await _context.accessCodes
                                           .FirstOrDefaultAsync(a => a.code == code);
            if (accessCode == null)
                return 0;

            var employee = await _context.agentes
                                         .FirstOrDefaultAsync(e => e.Id == accessCode.IdAgente);

            if (employee == null)
                return 0;

            var today = DateTime.Today;

            var histories = await _context.histories
                                          .Where(h => h.IdAngente == employee.Id)
                                          .ToListAsync();

            var punchCountToday = histories
                                  .Count(h => DateTime.TryParse(h.AdmissionDate, out DateTime admissionDate) &&
                                              admissionDate.Date == today && h.ExitDate != null);

            return punchCountToday;
        }

    }
}
