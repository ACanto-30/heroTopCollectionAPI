using Microsoft.AspNetCore.Mvc;
using heroTopCollectionAPI.Models;
using heroTopCollectionAPI.Services;
using heroTopCollectionAPI.Utilities;
using heroTopCollectionAPI.Attributes;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

namespace heroTopCollectionAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usuarioService;
        private readonly HashPassword _hashPassword;
        private readonly CreateToken _createToken;


        public UsersController(UsersService usuarioService, HashPassword hashPassword, CreateToken createToken)
        {
            _usuarioService = usuarioService;
            _hashPassword = hashPassword;
            _createToken = createToken;
        }


        [HttpPost("user/login")]
        [EndpointRule("application/json", requiresAuthorization: false)]
        public async Task<IActionResult> AuthenticateUser([FromBody] Users user)
        {
            return await _usuarioService.AuthenticateUser(user.Email, user.Password);
        }

        [HttpPost]
        [Route("user")]
        [EndpointRule("application/json", requiresAuthorization: false)]
        public async Task<IActionResult> PostUser(Users user)
        {
            string salt = _hashPassword.GenerateSalt();

            string hashedPassword = _hashPassword.HashPasswordWithSalt(user.Password, salt);

            string userToken = _createToken.GenerateToken(hashedPassword);

            Console.WriteLine(userToken);
            Console.Write(hashedPassword);

            return await _usuarioService.PostUser(user.Email, hashedPassword, userToken, salt);
        }

        [HttpPost("user/order")]
        [EndpointRule("application/json", requiresAuthorization: true)]
        public async Task<IActionResult> AddOrder([FromBody] ProductOrder[] order)
        {
            // Obtener el token directamente desde los headers
            string token = Request.Headers["Authorization"];

            return await _usuarioService.AddOrdersToUser(token, order);
        }

        [HttpPost("user/orders")]
        [EndpointRule("application/json", requiresAuthorization: true)]
        public async Task<IActionResult> AddOrders([FromBody] ProductOrder[] orders)
        {
            // Obtener el token directamente desde los headers
            string token = Request.Headers["Authorization"];

            return await _usuarioService.AddOrdersToUser(token, orders);
        }


        [HttpPost("user/verify")]
        [EndpointRule("application/json", requiresAuthorization: false)]
        public async Task<IActionResult> GetUser([FromHeader] string Authorization)
        {
            Console.WriteLine($"{Authorization}");

            // Verificar si el token es válido usando el servicio


            if (await _usuarioService.IsTokenValid(Authorization))
            {
                // Si el token es válido, retornar OK con el token
                return Ok(new { token = Authorization });
            }
            else
            {
                // Si el token no es válido, retornar BadRequest con un mensaje de error
                return BadRequest(new { Message = "Invalid token" });
            }
        }



    }
}