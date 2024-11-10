using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using task_management.Models;
using Serilog;

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
        public async Task<ActionResult<User>> Register(User user, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state during user registration");
                return BadRequest(ModelState);
            }

            var emailExists = await _context.User
                .AnyAsync(u => u.Email == user.Email, cancellationToken);
            if (emailExists)
            {
                Log.Information("Registration attempt with existing email: {Email}", user.Email);
                return BadRequest(new { error = "Email already registered." });
            }

            try
            {
                user.Role = UserRole;
                _context.User.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                Log.Information("User registered successfully: {UserId}", user.Id);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
                {
                    message = "User created successfully.",
                    user = new { user.Id, user.Name, user.Email, user.Role }
                });
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Error occurred while saving user to database");
                return StatusCode(500, new { error = "An error occurred while saving to the database." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error during user registration");
                return StatusCode(500, new { error = "An error occurred during registration." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id, CancellationToken cancellationToken)
        {
            var user = await _context.User.FindAsync(new object[] { id }, cancellationToken);
            if (user == null)
            {
                Log.Information("GetUser request for non-existent user: {UserId}", id);
                return NotFound();
            }

            Log.Information("User retrieved: {UserId}", id);
            return user;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(CancellationToken cancellationToken)
        {
            try
            {
                var users = await _context.User.ToListAsync(cancellationToken);
                Log.Information("Retrieved {UserCount} users", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while retrieving users");
                return StatusCode(500, new { error = "Failed to retrieve users." });
            }
        }

        [HttpPost("login")]
         public async Task<ActionResult<string>> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Name) || string.IsNullOrEmpty(loginRequest.Password))
            {
                Log.Warning("Login attempt with missing credentials");
                return BadRequest("Username and password are required.");
            }

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Name == loginRequest.Name);
            if (user == null || user.Password != loginRequest.Password)
            {
                Log.Warning("Failed login attempt for user: {UserName}", loginRequest.Name);
                return Unauthorized(new { error = "Invalid username or password." });
            }

            try
            {
                var token = GenerateJwtToken(user);
                Log.Information("Successful login for user: {UserId}", user.Id);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating JWT token for user: {UserId}", user.Id);
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