using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet("isAuthenticated")]
        [Authorize] // Требует аутентификации
        public IActionResult IsAuthenticated()
        {
            // Если пользователь аутентифицирован, возвращаем его имя
            return Ok(User.Identity.Name);
        }
    }
}
