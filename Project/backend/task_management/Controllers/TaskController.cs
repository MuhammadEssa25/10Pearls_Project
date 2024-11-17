using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task_management.Models;
using System.Security.Claims;
using Serilog;

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
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasks([FromQuery] TaskFilter filter)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                IQueryable<Models.Task> query = _context.Task
                    .Include(t => t.AssignedToUser)
                    .Where(t => !t.IsDeleted);

                if (userRole != "Admin")
                {
                    query = query.Where(t => t.AssignedToUserId == userId);
                }
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(t => t.Status == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.Priority))
                {
                    query = query.Where(t => t.Priority == filter.Priority);
                }

                if (filter.DueDate.HasValue)
                {
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == filter.DueDate.Value.Date);
                }

                var tasks = await query.ToListAsync();
                Log.Information("Retrieved {TaskCount} tasks for user {UserId}", tasks.Count, userId);
                return tasks;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving tasks");
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
                    Log.Information("Task not found: {TaskId}", id);
                    return NotFound(new { error = "Task not found." });
                }

                if (userRole != "Admin" && task.AssignedToUserId != userId)
                {
                    Log.Warning("Unauthorized access attempt to task {TaskId} by user {UserId}", id, userId);
                    return Forbid();
                }

                Log.Information("Task retrieved: {TaskId}", id);
                return task;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, new { error = "Failed to retrieve the task." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Models.Task>> CreateTask(Models.Task task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Title))
            {
                Log.Warning("Invalid task creation attempt");
                return BadRequest(new { error = "Task title is required." });
            }
            var assignedUser = await _context.User.FindAsync(task.AssignedToUserId);
            if (assignedUser == null)
            {
                Log.Warning("Task creation attempt with non-existent user: {UserId}", task.AssignedToUserId);
                return BadRequest(new { error = "Assigned user does not exist." });
            }

            try
            {
                task.AssignedToUser = assignedUser;
                _context.Task.Add(task);
                await _context.SaveChangesAsync();

                task.AssignedToUser = null;
                Log.Information("Task created: {TaskId}", task.Id);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating task");
                return StatusCode(500, new { error = "Failed to create task." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, Models.Task task)
        {
            if (id != task.Id)
            {
                Log.Warning("Task update attempt with mismatched IDs");
                return BadRequest(new { error = "Task ID mismatch." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var existingTask = await _context.Task.FindAsync(id);
            if (existingTask == null)
            {
                Log.Information("Update attempt for non-existent task: {TaskId}", id);
                return NotFound(new { error = "Task not found." });
            }

            if (userRole != "Admin" && existingTask.AssignedToUserId != userId)
            {
                Log.Warning("Unauthorized task update attempt: {TaskId} by user {UserId}", id, userId);
                return Forbid();
            }

            if (userRole != "Admin")
            {
                existingTask.Status = task.Status;
            }
            else
            {
                _context.Entry(existingTask).CurrentValues.SetValues(task);
            }

            try
            {
                await _context.SaveChangesAsync();
                Log.Information("Task updated: {TaskId}", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TaskExists(id))
                {
                    Log.Information("Concurrency error: Task not found {TaskId}", id);
                    return NotFound(new { error = "Task not found." });
                }
                else
                {
                    Log.Error(ex, "Concurrency error updating task {TaskId}", id);
                    return StatusCode(500, new { error = "Failed to update the task due to concurrency issues." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating task {TaskId}", id);
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
                    Log.Information("Delete attempt for non-existent task: {TaskId}", id);
                    return NotFound(new { error = "Task not found." });
                }

                task.IsDeleted = true;
                await _context.SaveChangesAsync();

                Log.Information("Task soft deleted: {TaskId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error soft deleting task {TaskId}", id);
                return StatusCode(500, new { error = "Failed to delete the task." });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<TaskCounts>> GetTaskCounts()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                
                Log.Information("Retrieving task counts for user: {UserId}", userId);

                var counts = new TaskCounts
                {
                    Completed = await _context.Task.CountAsync(t => t.AssignedToUserId == userId && t.Status == "Completed"),
                    InProgress = await _context.Task.CountAsync(t => t.AssignedToUserId == userId && t.Status == "In Progress"),
                    Pending = await _context.Task.CountAsync(t => t.AssignedToUserId == userId && t.Status == "Pending")
                };

                Log.Information("Task counts retrieved for user {UserId}: Completed={Completed}, InProgress={InProgress}, Pending={Pending}",
                    userId, counts.Completed, counts.InProgress, counts.Pending);

                return Ok(counts);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving task counts for user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { error = "Failed to retrieve task counts." });
            }
        }

        private bool TaskExists(int id)
        {
            return _context.Task.Any(e => e.Id == id);
        }

        internal async System.Threading.Tasks.Task GetTasks()
        {
            throw new NotImplementedException();
        }
    }

    public class TaskCounts
    {
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Pending { get; set; }
    }

    public class TaskFilter
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}