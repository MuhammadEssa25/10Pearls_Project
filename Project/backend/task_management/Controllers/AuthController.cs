using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using task_management.Models;

namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        private const string AdminRole = "Admin";
        private const string UserRole = "User";

        public AuthController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.User.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { error = "Email already registered." });

            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Role = UserRole; // Default role

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
                {
                    message = "User created successfully.",
                    user = new { user.Id, user.Name, user.Email, user.Role }
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "An error occurred while saving to the database." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred during registration." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound();

            return user;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                return await _context.User.ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Failed to retrieve users." });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Name) || string.IsNullOrEmpty(loginRequest.Password))
                return BadRequest("Username and password are required.");

            var user = await _context.User.FirstOrDefaultAsync(u => u.Name == loginRequest.Name);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                return Unauthorized(new { error = "Invalid username or password." });

            try
            {
                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Error generating JWT token." });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT configuration is invalid.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public required string Name { get; set; }
        public required string Password { get; set; }
    }
}