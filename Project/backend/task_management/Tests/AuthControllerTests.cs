using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using task_management.Controllers;
using task_management.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.AspNetCore.Http;
using Task = System.Threading.Tasks.Task;
using System.Linq.Expressions;

namespace task_management.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockContext = new Mock<ApplicationDBContext>();
            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new AuthController(_mockContext.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            var user = new User { Name = "John", Email = "john@example.com", Password = "password123" };

            // Act
            var result = await _controller.Register(user, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var user = new User { Name = "John", Email = "john@example.com", Password = "password123" };

            // Mocking the 'AnyAsync' call to return true (simulating email already exists)
            _mockContext.Setup(c => c.User.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true); 

            // Act
            var result = await _controller.Register(user, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var user = new User { Name = "John", Email = "john@example.com", Password = "password123" };

            // Mocking 'AddAsync' to return an EntityEntry object
            var entityEntryMock = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User>>();
            _mockContext.Setup(c => c.User.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(entityEntryMock.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.Register(user, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest { Name = "John", Password = "wrongpassword" };

            _mockContext.Setup(c => c.User.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((User)null); // Simulate invalid user.

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken_WhenValidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest { Name = "John", Password = "password123" };

            var user = new User { Id = 1, Name = "John", Email = "john@example.com", Password = "password123", Role = "User" };

            _mockContext.Setup(c => c.User.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(user); // Simulate valid user.

            _mockConfiguration.SetupGet(c => c["Jwt:Key"]).Returns("your_secret_key");
            _mockConfiguration.SetupGet(c => c["Jwt:Issuer"]).Returns("your_issuer");
            _mockConfiguration.SetupGet(c => c["Jwt:Audience"]).Returns("your_audience");

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.True(response.ContainsKey("token"));
        }
    }
}
