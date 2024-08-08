using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using puncherTng.ContextDB;
using puncherTng.Models;
using System.Text;

namespace puncherTng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Backoffice : ControllerBase
    {
        readonly Context _context;

        public Backoffice(Context context)
        {
            _context = context;
        }
        private static readonly Random _random = new Random();
        private const string _caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

        [HttpGet("History")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistories(string employee, DateTime from, DateTime to)
        {
            var query = from history in _context.histories
                        join agent in _context.agentes on history.IdAngente equals agent.Id
                        select new
                        {
                            HistoryId = history.Id,
                            AgentName = agent.Name,
                            fecha_entrada = history.AdmissionDate,
                            fecha_salida = history.ExitDate
                        };

            var result = await query.ToListAsync();
            var result_filter = (employee != "employee") ?
                result.Where(r => r.AgentName == employee
                                  && (TryParseDate(r.fecha_salida, out DateTime fechaSalida)
                                      ? fechaSalida >= from && fechaSalida <= to
                                      : r.fecha_salida == null)).ToList()
                : result.Where(r => (TryParseDate(r.fecha_salida, out DateTime fechaSalida)
                                      ? fechaSalida >= from && fechaSalida <= to
                                      : r.fecha_salida == null)).ToList();

            return Ok(result_filter);
        }

        private bool TryParseDate(string dateString, out DateTime date)
        {
            return DateTime.TryParse(dateString, out date);
        }


        [HttpGet("agente")]
        public async Task<ActionResult<IEnumerable<object>>> GetAgente()
        {
            try
            {
                var query = from agentes in _context.agentes
                            join code in _context.accessCodes on agentes.Id equals code.IdAgente
                            join companie in _context.companie on agentes.id_companie equals companie.Id
                            select new
                            {
                                id = agentes.Id,
                                name = agentes.Name,
                                lastname = agentes.LastName,
                                cedula = agentes.Cedula,
                                designation = agentes.designation,
                                correo = agentes.Correo,
                                codigo = code.code,
                                status = agentes.status,
                                name_companie = companie.name,
                                id_companie = companie.Id,
                            };
                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("agente/crear")]
        public async Task<IActionResult> CreateAgentAndCode(Agentes data)
        {
            if (data is null)
                return BadRequest("Data required");
            if (data.Name is null)
                return BadRequest("Name required");
            if (data.LastName is null)
                return BadRequest("LastName required");
            if (data.Cedula is null)
                return BadRequest("Cedula required");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string insertSql = "INSERT INTO agentes (Name,LastName,id_companie,Correo,Cedula,designation) VALUES (@name,@lastname,@id_companie,@correo,@cedula,@designatio); SELECT CAST(SCOPE_IDENTITY() as int);";

                SqlParameter[] parameters =
                {
                  new SqlParameter("@name", data.Name),
                  new SqlParameter("@lastname", data.LastName),
                  new SqlParameter("@id_companie", data.id_companie),
                  new SqlParameter("@cedula", data.Cedula),
                  new SqlParameter("@correo", data.Correo),
                  new SqlParameter("@designatio", data.designation)
                };

                var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = insertSql;
                command.Parameters.AddRange(parameters);

                _context.Database.OpenConnection(); 
                command.Transaction = transaction.GetDbTransaction(); 

                var result = await command.ExecuteScalarAsync();
                var insertedId = Convert.ToInt32(result);

                var insertedData = await _context.agentes.FindAsync(insertedId);

                if (insertedData != null)
                {
                    await CreateCodeForEmployee(insertedData);
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Empleado registrado",
                    data = insertedData
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error saving data to the database: " + ex.Message);
            }
        }

        [HttpPut("update/agente")]
        public async Task<IActionResult> updateAgente([FromBody] AgenteInput data, int id)
        {
            if (id == 0)
                return BadRequest("Id requiered");

            var dataupdate = await _context.agentes.FindAsync(id);

            if (dataupdate == null)
                return BadRequest("Agente Not Found");
            if (data is null)
                return BadRequest("Data requiered");

            dataupdate.id_companie = data.id_companie;
            dataupdate.Name = data.name;
            dataupdate.LastName = data.lastName;
            dataupdate.Correo = data.correo;
            dataupdate.Cedula = data.cedula;
            dataupdate.designation = data.Designatio;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Employee update",
                id = id
            });

        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<object>>> GetUser()
        {
            try
            {
                return await _context.usuarios.ToArrayAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create" + "user")]
        public async Task<IActionResult> CreateUser([FromBody] UserInput data)
        {
            if (data == null)
                return BadRequest("data requiered");
            if (data.username is null)
                return BadRequest("username requiered");
            if (data.email is null)
                return BadRequest("email requiered");
            if (data.cedula is null)
                return BadRequest("cedula requiered");

            var password = GenerarContraseña(7);
            var Data = new Usuarios
            {
                username = data.username,
                email = data.email,
                password = password,
                cedula = data.cedula,
                status = "Activo"
            };

            _context.usuarios.Add(Data);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Usuario registrado",
                Data.password,
                Datos = data
            });
        }


        public static string GenerarContraseña(int longitud)
        {
            if (longitud < 4) throw new ArgumentException("La longitud de la contraseña debe ser al menos 4 para incluir todos los tipos de caracteres");

            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string digitos = "0123456789";
            const string especiales = "!@#$%^&*()";

            var contraseña = new StringBuilder(longitud);
            var todosCaracteres = mayusculas + minusculas + digitos + especiales;


            contraseña.Append(mayusculas[_random.Next(mayusculas.Length)]);
            contraseña.Append(minusculas[_random.Next(minusculas.Length)]);
            contraseña.Append(digitos[_random.Next(digitos.Length)]);
            contraseña.Append(especiales[_random.Next(especiales.Length)]);

            for (int i = 4; i < longitud; i++)
            {
                contraseña.Append(todosCaracteres[_random.Next(todosCaracteres.Length)]);
            }

            return new string(contraseña.ToString().OrderBy(c => _random.Next()).ToArray());
        }


        [HttpPut("update/user")]

        public async Task<IActionResult> UpdateUser([FromBody] UserInput data, int id)
        {
            var dataUser = await _context.usuarios.FindAsync(id);

            if (dataUser == null)
                return BadRequest("User Not Found");
            if (data == null)
                return BadRequest("Data requiered");


            dataUser.username = data.username;
            dataUser.email = data.email;
            dataUser.cedula = data.cedula;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "User update correct",
                id
            });
        }

        [HttpPut("update/status")]
        public async Task<IActionResult> UpdateStatusUser([FromBody] string status, int id)
        {
            var dataUser = await _context.usuarios.FindAsync(id);

            if (dataUser == null)
                return BadRequest("User Not Found");
            if (status == null)
                return BadRequest("Status requiered");

            if (dataUser.status == status)
            {
                dataUser.status = dataUser.status.ToString();
            }
            else if (dataUser.status != status)
            {
                dataUser.status = status;
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "User status update correct",
                id
            });
        }

        [HttpPut("update/status/employee")]
        public async Task<IActionResult> UpdateStatusEmployee([FromBody] string status, int id)
        {
            var dataEmplyee = await _context.agentes.FindAsync(id);

            if (dataEmplyee == null)
                return BadRequest("User Not Found");
            if (status == null)
                return BadRequest("Status requiered");

            if (dataEmplyee.status == status)
            {
                dataEmplyee.status = dataEmplyee.status.ToString();
            }
            else if (dataEmplyee.status != status)
            {
                dataEmplyee.status = status;
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Employee status update correct",
                id
            });
        }

        [HttpGet("code/access")]
        public async Task<ActionResult<IEnumerable<object>>> GetAccessCode()
        {
            try
            {
                var query = from code in _context.accessCodes
                            join agen in _context.agentes on code.IdAgente equals agen.Id
                            select new
                            {
                                name = agen.Name,
                                cedula = agen.Cedula,
                                code = code.code,
                            };
                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("compnies")]
        public async Task<ActionResult<IEnumerable<object>>> GetCompanies()
        {
            try
            {
                return await _context.companie.ToArrayAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("create/companie")]
        public async Task<IActionResult> CreateCompanie([FromBody] InputCompanies data)
        {
            if (data == null)
                return BadRequest("Data requierd");
            if (data.name == null)
                return BadRequest("name requeired");


            var companie = new Companies
            {
                name = data.name,
                code_identification = data.code_identification
            };

            _context.companie.Add(companie);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "companie register correct",
                data = data
            });
        }

        [HttpPut("update/companie")]
        public async Task<IActionResult> UpdateCompanie([FromBody] InputCompanies data, int id)
        {
            var data_companie = await _context.companie.FindAsync(id);

            if (data_companie == null)
                return BadRequest("User Not Found");
            if (data == null)
                return BadRequest("Data requiered");

            data_companie.name = data.name;
            data_companie.code_identification = data.code_identification;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "update companie correct",
                id = id
            });
        }

        [HttpPost("code")]
        public async Task<IActionResult> CreateCodeForEmployee(Agentes data)
        {
            if (data == null || string.IsNullOrEmpty(data.Name) || data.id_companie <= 0)
                return BadRequest("Data required");

            try
            {
                var code = await GenerateCode(data.Name, data.id_companie);
                var Data = new AccessCode
                {
                    IdAgente = data.Id,
                    code = code
                };
                _context.accessCodes.Add(Data);
                await _context.SaveChangesAsync();

                return Ok(Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<string> GenerateCode(string name, int companie_id)
        {
            var companie = await _context.companie.FindAsync(companie_id);

            if (companie == null)
                throw new Exception("Company not found");

            var random = new Random();
            var number1 = random.Next(0, 1000);
            var firstLetterName = name[0];

            var code = $"{companie.code_identification}-{firstLetterName}{number1:000}";
            return code;
        }
    }
}
