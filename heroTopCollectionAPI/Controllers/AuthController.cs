using heroTopCollectionAPI.Services;
using heroTopCollectionAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Storage.v1.Data;
using Microsoft.AspNetCore.Authorization;
using heroTopCollectionAPI.Attributes;

namespace heroTopCollectionAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: api/auth/sign-in
        [HttpGet("sign-in")]
        [EndpointRule("", requiresAuthorization: false)]
        public IActionResult Login()
        {
            // Especifica la ruta completa si la vista está en un subdirectorio
            return View("~/Views/Auth/sign-in/SignInView.cshtml");
        }

        // POST: api/auth/sign-in
        [HttpPost("sign-in")]
        [EndpointRule("application/json", requiresAuthorization: false)]
        public async Task<IActionResult> Login(Users user)
        {
            var result = await _authService.LoginUserAsync(user, HttpContext);
            Console.WriteLine(result);

            // Verifica si el inicio de sesión fue exitoso
            if (!string.IsNullOrEmpty(result))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Error al iniciar sesión");
                return View("~/Views/Auth/sign-in/SignInView.cshtml");
            }
        }

        [HttpPost("sign-up")]
        [EndpointRule("application/json", requiresAuthorization: false)]
        public async Task<IActionResult> Register([FromBody] Users user)
        {
            if (user == null)
            {
                return BadRequest("El usuario no puede ser nulo");
            }

            var result = await _authService.RegisterUserAsync(user, HttpContext);
            Console.WriteLine(result);

            // Verifica si el registro fue exitoso
            if (!string.IsNullOrEmpty(result))
            {
                return Ok(new { message = "Usuario registrado con éxito" });
            }
            else
            {
                return BadRequest("Error al registrar usuario");
            }
        }

        // GET: api/auth/sign-up
        [HttpGet("sign-up")]
        [EndpointRule("", requiresAuthorization: false)]
        public IActionResult Register()
        {
            // Especifica la ruta completa si la vista está en un subdirectorio
            return View("~/Views/Auth/sign-up/SignUpView.cshtml");
        }

        [HttpPost("logout")]
        [EndpointRule("application/json", requiresAuthorization: true)]
        public IActionResult Logout()
        {
            // Eliminar la cookie de autenticación
            HttpContext.Response.Cookies.Delete("authToken");

            // Redirigir al usuario a la página de inicio
            return RedirectToAction("Index", "Home");
        }


    }
}