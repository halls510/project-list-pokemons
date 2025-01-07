using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using project_list_pokemons.Api.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace project_list_pokemons.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Realiza o login e retorna um token JWT.
        /// </summary>
        /// <param name="loginRequest">Dados de login.</param>
        /// <returns>Token JWT ou erro de autenticação.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // Usuário e senha fixos para teste
            if (loginRequest.Username == "admin" && loginRequest.Password == "123456789")
            {
                var token = GenerateJwtToken(loginRequest.Username);
                return Ok(new ApiResponseToken(
                            token: token,
                            statusCode: "200_OK"
                        ));
            }

            return Unauthorized(new ApiResponse(
                                 message: "Credenciais inválidas.",
                                 statusCode: "401_UNAUTHORIZED"
                             ));
        }

        private string GenerateJwtToken(string username)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = jwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
