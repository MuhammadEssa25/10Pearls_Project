using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using task_management.Controllers;
using task_management.Models;
using Task = System.Threading.Tasks.Task;

namespace task_management.Tests
{
    public class TaskControllerTests
    {
        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            _mockContext = new Mock<ApplicationDBContext>();
            _controller = new TaskController(_mockContext.Object);

            // Setup ClaimsPrincipal for authorization
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetTasks_ReturnsTaskList()
        {
            // Arrange
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = 1, Title = "Task 1", AssignedToUserId = 1 },
                new Models.Task { Id = 2, Title = "Task 2", AssignedToUserId = 1 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Models.Task>>();
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.Provider).Returns(tasks.Provider);
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.Expression).Returns(tasks.Expression);
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());

            _mockContext.Setup(c => c.Task).Returns(mockSet.Object);

            var filter = new TaskFilter(); // Create an empty filter

            // Act
            var result = await _controller.GetTasks(filter);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Models.Task>>>(result);
            var returnValue = Assert.IsType<List<Models.Task>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetTask_ExistingTask_ReturnsTask()
        {
            // Arrange
            var taskId = 1;
            var task = new Models.Task { Id = taskId, Title = "Test Task", AssignedToUserId = 1 };
            _mockContext.Setup(c => c.Task.FindAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _controller.GetTask(taskId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Models.Task>>(result);
            var returnValue = Assert.IsType<Models.Task>(actionResult.Value);
            Assert.Equal(taskId, returnValue.Id);
        }

        [Fact]
        public async Task CreateTask_ValidTask_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var task = new Models.Task { Title = "New Task", AssignedToUserId = 1 };
            var user = new User { Id = 1, Name = "TestUser" };
            _mockContext.Setup(c => c.User.FindAsync(task.AssignedToUserId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.CreateTask(task);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetTask", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<Models.Task>(createdAtActionResult.Value);
            Assert.Equal(task.Title, returnValue.Title);
        }

        [Fact]
        public async Task UpdateTask_ValidTask_ReturnsNoContent()
        {
            // Arrange
            var taskId = 1;
            var task = new Models.Task { Id = taskId, Title = "Updated Task", AssignedToUserId = 1 };
            _mockContext.Setup(c => c.Task.FindAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _controller.UpdateTask(taskId, task);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ExistingTask_ReturnsNoContent()
        {
            // Arrange
            var taskId = 1;
            var task = new Models.Task { Id = taskId, Title = "Task to Delete" };
            _mockContext.Setup(c => c.Task.FindAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _controller.DeleteTask(taskId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetTaskCounts_ReturnsTaskCounts()
        {
            // Arrange
            var userId = 1;
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = 1, AssignedToUserId = userId, Status = "Completed" },
                new Models.Task { Id = 2, AssignedToUserId = userId, Status = "Completed" },
                new Models.Task { Id = 3, AssignedToUserId = userId, Status = "In Progress" },
                new Models.Task { Id = 4, AssignedToUserId = userId, Status = "In Progress" },
                new Models.Task { Id = 5, AssignedToUserId = userId, Status = "Pending" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Models.Task>>();
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.Provider).Returns(tasks.Provider);
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.Expression).Returns(tasks.Expression);
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
            mockSet.As<IQueryable<Models.Task>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());

            _mockContext.Setup(c => c.Task).Returns(mockSet.Object);

            // Set up the user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetTaskCounts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var taskCounts = Assert.IsType<TaskCounts>(okResult.Value);
            Assert.Equal(2, taskCounts.Completed);
            Assert.Equal(2, taskCounts.InProgress);
            Assert.Equal(1, taskCounts.Pending);
        }
    }
}