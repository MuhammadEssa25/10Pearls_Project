using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using task_management.Controllers;
using task_management.Models;
using task_management.Services;
using Task = System.Threading.Tasks.Task;

namespace task_management.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<PasswordService> _mockPasswordService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockContext = new Mock<ApplicationDBContext>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockPasswordService = new Mock<PasswordService>();

            _controller = new AuthController(
                _mockContext.Object,
                _mockConfiguration.Object,
                _mockPasswordService.Object
            );
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var user = new User { Name = "TestUser", Email = "test@example.com", Password = "password123" };
            _mockContext.Setup(c => c.User.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockPasswordService.Setup(s => s.HashPassword(It.IsAny<string>())).Returns("hashedPassword");

            // Act
            var result = await _controller.Register(user, CancellationToken.None);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetUser", createdAtActionResult.ActionName);
            Assert.NotNull(createdAtActionResult.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResultWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequest { Name = "TestUser", Password = "password123" };
            var user = new User { Id = 1, Name = "TestUser", Password = "hashedPassword", Role = "User" };
            _mockContext.Setup(c => c.User.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockPasswordService.Setup(s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockConfiguration.SetupGet(c => c["Jwt:Key"]).Returns("your-secret-key");
            _mockConfiguration.SetupGet(c => c["Jwt:Issuer"]).Returns("your-issuer");
            _mockConfiguration.SetupGet(c => c["Jwt:Audience"]).Returns("your-audience");

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            Assert.IsType<string>(((dynamic)okResult.Value).token);
        }

        [Fact]
        public async Task GetUser_ExistingUser_ReturnsUser()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Name = "TestUser", Email = "test@example.com" };
            _mockContext.Setup(c => c.User.FindAsync(new object[] { userId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<ActionResult<User>>(result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task GetUser_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            _mockContext.Setup(c => c.User.FindAsync(new object[] { userId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}