using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_management.Models;
using System.Security.Claims;

namespace task_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public TaskController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasks()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Models.Task> query = _context.Task.Include(t => t.AssignedToUser);

                if (userRole != "Admin")
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }

                var tasks = await query.ToListAsync();
                return tasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving tasks: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve tasks." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Task>> GetTask(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var task = await _context.Task
                    .Include(t => t.AssignedToUser)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound(new { error = "Task not found." });
                }

                if (userRole != "Admin" && task.AssignedToUserId != userId)
                {
                    return Forbid();
                }

                return task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving task {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve the task." });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<TaskCounts>> GetTaskCounts()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var counts = new TaskCounts
                {
                    Completed = await _context.Task.CountAsync(t => t.AssignedToUserId == userId && t.Status == "Completed"),
                    InProgress = await _context.Task.CountAsync(t => t.AssignedToUserId == userId && t.Status == "In Progress"),
                    Pending = await _context.Task.CountAsync(t => t.AssignedToUserId == userId && t.Status == "Pending")
                };

                return Ok(counts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving task counts: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve task counts." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Models.Task>> CreateTask(Models.Task task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Title))
            {
                return BadRequest(new { error = "Task title is required." });
            }
            var assignedUser = await _context.User.FindAsync(task.AssignedToUserId);
            if (assignedUser == null)
            {
                return BadRequest(new { error = "Assigned user does not exist." });
            }

            try
            {
                task.AssignedToUser = assignedUser;
                _context.Task.Add(task);
                await _context.SaveChangesAsync();

                task.AssignedToUser = null;
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating task: {ex.Message}");
                return StatusCode(500, new { error = "Failed to create task." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Models.Task task)
        {
            if (id != task.Id)
            {
                return BadRequest(new { error = "Task ID mismatch." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var existingTask = await _context.Task.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound(new { error = "Task not found." });
            }

            if (userRole != "Admin" && existingTask.AssignedToUserId != userId)
            {
                return Forbid();
            }

            if (userRole != "Admin")
            {
                // Regular users can only update the status
                existingTask.Status = task.Status;
            }
            else
            {
                // Admins can update all fields
                _context.Entry(existingTask).CurrentValues.SetValues(task);
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound(new { error = "Task not found." });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to update the task due to concurrency issues." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to update the task." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.Task.FindAsync(id);
                if (task == null)
                {
                    return NotFound(new { error = "Task not found." });
                }

                _context.Task.Remove(task);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to delete the task." });
            }
        }

        private bool TaskExists(int id)
        {
            return _context.Task.Any(e => e.Id == id);
        }
    }

    public class TaskCounts
    {
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Pending { get; set; }
    }
}