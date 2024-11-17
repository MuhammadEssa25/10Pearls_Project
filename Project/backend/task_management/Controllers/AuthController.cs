using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using task_management.Models;
using task_management.Services;
using Serilog;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;
        private const string AdminRole = "Admin";
        private const string UserRole = "User";

        public AuthController(ApplicationDBContext context, IConfiguration configuration, PasswordService passwordService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
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
                .AnyAsync(u => u.Email == user.Email && !u.IsDeleted, cancellationToken);
            if (emailExists)
            {
                Log.Information("Registration attempt with existing email: {Email}", user.Email);
                return BadRequest(new { error = "Email already registered." });
            }

            try
            {
                user.Role = UserRole;
                user.Password = _passwordService.HashPassword(user.Password);
                _context.User.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                Log.Information("User registered successfully: {UserId}", user.Id);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
                {
                    message = "User created successfully.",
                    user = new { user.Id, user.Name, user.Email, user.Role }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during user registration");
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
        //Automatically creates a user with admin right 
        [HttpPost("ensure-admin")]
        public async Task<ActionResult> EnsureAdminExists(CancellationToken cancellationToken)
        {
        try
        {
        var adminExists = await _context.User
            .AnyAsync(u => u.Name == "admin" && u.Role == AdminRole && !u.IsDeleted, cancellationToken);

        if (!adminExists)
        {
            var hashedPassword = _passwordService.HashPassword("admin");
            var adminUser = new User
            {
                Name = "admin",
                Password = hashedPassword,
                Role = AdminRole,
                Email = "admin@default.com", 
            };

            _context.User.Add(adminUser);
            await _context.SaveChangesAsync(cancellationToken);

            Log.Information("Admin user created successfully.");
            return Ok(new { message = "Admin user created successfully." });
        }

        Log.Information("Admin user already exists.");
        return Ok(new { message = "Admin user already exists." });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error occurred while ensuring admin user exists.");
        return StatusCode(500, new { error = "An error occurred while ensuring admin user exists." });
    }
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
                .FirstOrDefaultAsync(u => u.Name == loginRequest.Name && !u.IsDeleted);
            if (user == null || !_passwordService.VerifyPassword(loginRequest.Password, user.Password))
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