using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using task_management.Controllers;
using task_management.Models;
using Xunit;
using ModelTask = task_management.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace task_management.Tests
{
    public class TaskControllerTests
    {
        private readonly Mock<ApplicationDBContext> _mockContext;
        private readonly TaskController _controller;
        private readonly Mock<DbSet<ModelTask>> _mockTaskSet;

        public TaskControllerTests()
        {
            _mockTaskSet = new Mock<DbSet<ModelTask>>();
            _mockContext = new Mock<ApplicationDBContext>();
            
            _mockContext.Setup(m => m.Task).Returns(_mockTaskSet.Object);

            _controller = new TaskController(_mockContext.Object);

            // Setup ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Setup ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private ClaimsPrincipal CreateClaimsPrincipal(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task CreateTask_ShouldReturnCreatedAtAction_WhenTaskCreatedSuccessfully()
        {
            // Arrange
            var newTask = new ModelTask
            {
                Id = 1,
                Title = "Sample Task",
                Description = "Task Description",
                DueDate = DateTime.UtcNow.AddDays(1),
                Priority = "High",
                Status = "Pending",
                AssignedToUserId = 1
            };

            _mockContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockTaskSet.Setup(m => m.Add(It.IsAny<ModelTask>())).Verifiable();

            // Act
            var result = await _controller.CreateTask(newTask);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be("GetTask");
            createdResult.Value.Should().BeEquivalentTo(newTask);

            // Verify Add method was called
            _mockTaskSet.Verify(m => m.Add(It.IsAny<ModelTask>()), Times.Once);
        }

        [Fact]
        public async Task GetTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            int taskId = 1;
            _mockTaskSet.Setup(m => m.FindAsync(taskId)).ReturnsAsync((ModelTask)null);

            // Act
            var result = await _controller.GetTask(taskId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetTask_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            var existingTask = new ModelTask
            {
                Id = 1,
                Title = "Existing Task",
                Description = "Description",
                Priority = "Normal",
                Status = "Pending",
                AssignedToUserId = 1
            };

            _mockTaskSet.Setup(m => m.FindAsync(existingTask.Id)).ReturnsAsync(existingTask);

            // Act
            var result = await _controller.GetTask(existingTask.Id);

            // Assert
            result.Value.Should().BeEquivalentTo(existingTask);
        }

        [Fact]
        public async Task GetTasks_ShouldReturnAllTasks_WhenUserIsAdmin()
        {
            // Arrange
            var tasks = new List<ModelTask>
            {
                new ModelTask { Id = 1, Title = "Task 1", Description = "Description 1", AssignedToUserId = 1 },
                new ModelTask { Id = 2, Title = "Task 2", Description = "Description 2", AssignedToUserId = 2 }
            }.AsQueryable();

            _mockTaskSet.As<IQueryable<ModelTask>>().Setup(m => m.Provider).Returns(tasks.Provider);
            _mockTaskSet.As<IQueryable<ModelTask>>().Setup(m => m.Expression).Returns(tasks.Expression);
            _mockTaskSet.As<IQueryable<ModelTask>>().Setup(m => m.ElementType).Returns(tasks.ElementType);
            _mockTaskSet.As<IQueryable<ModelTask>>().Setup(m => m.GetEnumerator()).Returns(tasks.GetEnumerator());

            // Set user as Admin
            _controller.ControllerContext.HttpContext.User = CreateClaimsPrincipal("Admin");

            // Act
            var result = await _controller.GetTasks();

            // Assert
            result.Value.Should().BeEquivalentTo(tasks.ToList());
        }

        [Fact]
        public async Task UpdateTask_ShouldReturnNoContent_WhenTaskUpdatedSuccessfully()
        {
            // Arrange
            var taskId = 1;
            var updatedTask = new ModelTask
            {
                Id = taskId,
                Title = "Updated Task",
                Description = "Updated Description",
                Priority = "High",
                Status = "InProgress",
                AssignedToUserId = 1
            };

            _mockTaskSet.Setup(m => m.FindAsync(taskId)).ReturnsAsync(updatedTask);
            _mockContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateTask(taskId, updatedTask);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTask_ShouldReturnNoContent_WhenTaskDeletedSuccessfully()
        {
            // Arrange
            int taskId = 1;
            var existingTask = new ModelTask { Id = taskId, Title = "Task to Delete" };

            _mockTaskSet.Setup(m => m.FindAsync(taskId)).ReturnsAsync(existingTask);
            _mockTaskSet.Setup(m => m.Remove(existingTask));
            _mockContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Set user as Admin
            _controller.ControllerContext.HttpContext.User = CreateClaimsPrincipal("Admin");

            // Act
            var result = await _controller.DeleteTask(taskId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
public async Task DeleteTask_ShouldReturnForbidden_WhenUserIsNotAdmin()
{
    // Arrange
    int taskId = 1;
    var existingTask = new ModelTask { Id = taskId, Title = "Task to Delete" };

    _mockTaskSet.Setup(m => m.FindAsync(taskId)).ReturnsAsync(existingTask);
    _mockTaskSet.Setup(m => m.Remove(existingTask));
    _mockContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

    // Set user as regular user
    _controller.ControllerContext.HttpContext.User = CreateClaimsPrincipal("User");

    // Act
    var result = await _controller.DeleteTask(taskId);

    // Assert
    result.Should().BeOfType<ForbidResult>();  // Corrected to ForbidResult
}
    }

}
