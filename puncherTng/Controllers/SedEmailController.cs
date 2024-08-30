using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using puncherTng.Models;
using System.Net.Mail;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace puncherTng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SedEmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SedEmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult SendEmail([FromBody] EmailInput request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.subject) || string.IsNullOrEmpty(request.email) || string.IsNullOrEmpty(request.message))
                {
                    return BadRequest(new { success = false, error = "Campos requeridos faltantes" });
                }

                var smtpSettings = _configuration.GetSection("SmtpSettings");
                string host = smtpSettings["Host"];
                int port = int.Parse(smtpSettings["Port"]);
                string user = smtpSettings["User"];
                string pass = smtpSettings["Pass"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    return StatusCode(500, new { success = false, error = "Configuración SMTP incompleta" });
                }

                var smtpClient = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = false,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(request.email),
                    Subject = $"Nuevo mensaje de {request.subject}",
                    Body = request.message,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add("correloteria@gmail.com");

                smtpClient.Send(mailMessage);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en el envío de correo: " + ex.ToString());
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
    
    
}
