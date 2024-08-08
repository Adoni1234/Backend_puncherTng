using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using puncherTng.ContextDB;
using puncherTng.Models;

namespace puncherTng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        private readonly Context _context;

        public Authentication(Context context)
        {
            _context = context;
        }

        [HttpPost("login")]

        public async Task<IActionResult> LoginAuth(AuthInputs data)
        {
            if (data == null)
                return BadRequest("Data Requiered");
            if(data.username is null)
                return BadRequest("Username Requiered");
            if (data.password is null)
                return BadRequest("password Requiered");

            var usernames = await _context.usuarios.FirstOrDefaultAsync(u => u.username == data.username && u.password == data.password);
            var message = (usernames == null)? "Datos invalidos" : "Inicio De Session Exitoso";

            return Ok(new { 
               message,
               usernames
            });

        }

        [HttpPost("register")]
         
        public async Task<IActionResult> SignUp(Usuarios data)
        {
            if(data == null)
                return BadRequest("Data Requiered");
            if (data.username is null)
                return BadRequest("Username requeired");
            if (data.password is null)
                return BadRequest("Password requeired");
            if (data.email is null)
                return BadRequest("Email requeired");
            _context.usuarios.Add(data);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuario Registrado Existosamente"});
        }


        [HttpPut("restore/Password")]

        public async Task<IActionResult> RestorePassword([FromBody] PasswordInput data)
        {
            if (data is null)
                return BadRequest("Data is null, data required");

            if (data.Id == 0)
                return BadRequest("Id Requiered");

            var dbInput = await _context.usuarios.FindAsync(data.Id);

            if (dbInput == null)
                return BadRequest("User not Found");

            if(dbInput.password == data.current_password)
            {
              dbInput.password = data.password;
            }
            else
            {
                return BadRequest("current password do not match");
            }

            await _context.SaveChangesAsync();
            return Ok(new { 
                 message = "Su contraseña a sido cambianda sastifactoriamente " + dbInput.username
            });


        }
    }
}
