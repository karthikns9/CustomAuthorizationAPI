using CustomAuthorizationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CustomAuthorizationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [MyAuthorize]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("get-admin-data")]
        //[Authorize("RequireAdminRole")]
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult GetAdminData()
        {
            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            var scopes = new List<string> { "Campaign", "Integrations", "Users" };
            var permissions = new List<string> { "create", "read", "delete", "update" };

            // Create claims for the user
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "john_doe"),
            new Claim(ClaimTypes.Role, "admin"),
            };

            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permissions", permission));
            }

            // Specify the key used to sign the token
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("3272357538782F4125442A472D4B6150645367566B5970337336763979244226"));

            // Create the signing credentials using the key and the algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: "https://dev-8uc18ljq3ybzi0w3.us.auth0.com/",
                audience: "https://glossary.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Token expiration time
                signingCredentials: creds
            );

            // Return the token as a string
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
